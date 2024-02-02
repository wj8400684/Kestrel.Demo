using System.Net.Sockets;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;

namespace SuperSocket.Kestrel;

internal sealed class KestrelSocketTransportFactory(
    ListenOptions options,
    IConnectionListenerFactory socketTransportFactory,
    Func<ConnectionContext, ValueTask<IChannel>> channelFactory,
    ILogger logger)
    : IChannelCreator
{
    private IConnectionListener _connectionListener;
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
            var listenEndpoint = Options.GetListenEndPoint();

            var result = socketTransportFactory.BindAsync(listenEndpoint);

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

    private async Task KeepAcceptAsync(IConnectionListener connectionListener)
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                var client = await connectionListener.AcceptAsync().ConfigureAwait(false);
                OnNewClientAccept(client);
            }
            catch (Exception e)
            {
                if (e is ObjectDisposedException or NullReferenceException)
                    break;

                if (e is SocketException se)
                {
                    var errorCode = se.ErrorCode;

                    //The listen socket was closed
                    if (errorCode == 125 || errorCode == 89 || errorCode == 995 || errorCode == 10004 || errorCode == 10038)
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
