using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;

namespace SuperSocket.Kestrel;

internal sealed class KestrelQuicTransportFactory(
    ListenOptions options,
    IMultiplexedConnectionListenerFactory socketTransportFactory,
    Func<ConnectionContext, ValueTask<IChannel>> channelFactory,
    ILogger logger)
    : IChannelCreator
{
    private IMultiplexedConnectionListener _connectionListener;
    private CancellationTokenSource _cancellationTokenSource;
    private TaskCompletionSource<bool> _stopTaskCompletionSource;

    public ListenOptions Options { get; } = options;

    public event NewClientAcceptHandler NewClientAccepted;

    public bool IsRunning { get; private set; }

    Task<IChannel> IChannelCreator.CreateChannel(object connection) => throw new NotImplementedException();

    bool IChannelCreator.Start()
    {
        try
        {
            var collection = new FeatureCollection();

            collection.Set(new TlsConnectionCallbackOptions
            {
                ApplicationProtocols =
                [
                    SslApplicationProtocol.Http3
                ],
                OnConnection = OnConnectionAsync
            });

            var listenEndpoint = Options.GetListenEndPoint();

            var result = socketTransportFactory.BindAsync(listenEndpoint, collection);

            _connectionListener = result.IsCompleted ? result.Result : result.GetAwaiter().GetResult();

            IsRunning = true;

            _cancellationTokenSource = new CancellationTokenSource();

            KeepAcceptAsync(_connectionListener).DoNotAwait();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, $"The listener[{this.ToString()}] failed to start.");
            return false;
        }
    }

    private ValueTask<SslServerAuthenticationOptions> OnConnectionAsync(TlsConnectionCallbackContext context, CancellationToken cancellationToken)
    {
        return new ValueTask<SslServerAuthenticationOptions>(new SslServerAuthenticationOptions
        {
            ApplicationProtocols = [SslApplicationProtocol.Http3],
            ServerCertificate = GenerateManualCertificate()
        });
    }

    X509Certificate2 GenerateManualCertificate()
    {
        X509Certificate2 cert = null;
        var store = new X509Store("KestrelWebTransportCertificates", StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        if (store.Certificates.Count > 0)
        {
            cert = store.Certificates[^1];

            // rotate key after it expires
            if (DateTime.Parse(cert.GetExpirationDateString(), null) < DateTimeOffset.UtcNow)
            {
                cert = null;
            }
        }
        if (cert == null)
        {
            // generate a new cert
            var now = DateTimeOffset.UtcNow;
            SubjectAlternativeNameBuilder sanBuilder = new();
            sanBuilder.AddDnsName("localhost");
            using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            CertificateRequest req = new("CN=localhost", ec, HashAlgorithmName.SHA256);
            // Adds purpose
            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
        {
            new("1.3.6.1.5.5.7.3.1") // serverAuth

        }, false));
            // Adds usage
            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
            // Adds subject alternate names
            req.CertificateExtensions.Add(sanBuilder.Build());
            // Sign
            using var crt = req.CreateSelfSigned(now, now.AddDays(14)); // 14 days is the max duration of a certificate for this
            cert = new(crt.Export(X509ContentType.Pfx));

            // Save
            store.Add(cert);
        }
        store.Close();

        var hash = SHA256.HashData(cert.RawData);
        var certStr = Convert.ToBase64String(hash);
        //Console.WriteLine($"\n\n\n\n\nCertificate: {certStr}\n\n\n\n"); // <-- you will need to put this output into the JS API call to allow the connection
        return cert;
    }

    Task IChannelCreator.StopAsync()
    {
        var listenSocket = _connectionListener;

        if (listenSocket == null)
            return Task.Delay(0);

        _stopTaskCompletionSource = new TaskCompletionSource<bool>();

        _cancellationTokenSource.Cancel();
        _connectionListener.UnbindAsync().DoNotAwait();

        return _stopTaskCompletionSource.Task;
    }

    private async Task KeepAcceptAsync(IMultiplexedConnectionListener connectionListener)
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                var multiplexedConnectionContext = await connectionListener.AcceptAsync().ConfigureAwait(false);

                if (multiplexedConnectionContext == null)
                {
                    logger.LogError($"Listener[{this.ToString()}] failed to do AcceptAsync");
                    continue;
                }
                
                var connectionContext = await multiplexedConnectionContext.AcceptAsync().ConfigureAwait(false);
                
                OnNewClientAccept(connectionContext);
            }
            catch (Exception e)
            {
                if (e is ObjectDisposedException or NullReferenceException)
                    break;

                if (e is SocketException se)
                {
                    var errorCode = se.ErrorCode;

                    //The listen socket was closed
                    if (errorCode == 125 || errorCode == 89 || errorCode == 995 || errorCode == 10004 ||
                        errorCode == 10038)
                    {
                        break;
                    }
                }

                logger.LogError(e, $"Listener[{this.ToString()}] failed to do AcceptAsync");
            }
        }

        _stopTaskCompletionSource.TrySetResult(true);
    }

    private async void OnNewClientAccept(ConnectionContext context)
    {
        var handler = NewClientAccepted;

        if (handler == null)
            return;

        IChannel channel = null;

        try
        {
            channel = await channelFactory(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to create channel for {context.RemoteEndPoint}.");
            return;
        }

        await handler.Invoke(this, channel);
    }

    public override string ToString()
    {
        return Options?.ToString();
    }
}