using System.IO.Pipes;
using System.Net.Sockets;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;

namespace SuperSocket.Kestrel.NamedPipe;

internal sealed class NamedPipeTransportFactory(
    ListenOptions options,
    Func<ConnectionContext, ValueTask<IChannel>> channelFactory,
    ILogger logger) : IChannelCreator
{
    private CancellationTokenSource _cancellationTokenSource;
    private TaskCompletionSource<bool> _stopTaskCompletionSource;

    Task<IChannel> IChannelCreator.CreateChannel(object connection) => throw new NotImplementedException();

    public ListenOptions Options { get; } = options;

    public event NewClientAcceptHandler NewClientAccepted;

    public bool IsRunning { get; private set; }

    public bool Start()
    {
        var options = Options;

        try
        {
            var listenEndpoint = new NamedPipeEndPoint("ss");

            IsRunning = true;

            _cancellationTokenSource = new CancellationTokenSource();

            KeepAccept(listenEndpoint).DoNotAwait();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, $"The listener[{this.ToString()}] failed to start.");
            return false;
        }
    }

    public Task StopAsync()
    {
        _stopTaskCompletionSource = new TaskCompletionSource<bool>();

        _cancellationTokenSource.Cancel();

        return _stopTaskCompletionSource.Task;
    }

    private async Task KeepAccept(NamedPipeEndPoint namedPipeEndPoint)
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                var stream = new NamedPipeServerStream(namedPipeEndPoint.PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await stream.WaitForConnectionAsync(_cancellationTokenSource.Token).ConfigureAwait(false);

                OnNewClientAccept(stream);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Listener[{this.ToString()}] failed to do AcceptAsync");
            }
        }

        _stopTaskCompletionSource.TrySetResult(true);
    }

    private async void OnNewClientAccept(PipeStream stream)
    {
        var handler = NewClientAccepted;

        if (handler == null)
            return;

        IChannel channel;

        try
        {
            channel = await channelFactory(new NamedPipeConnectionContext(stream));
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to create channel for .");
            return;
        }

        await handler.Invoke(this, channel);
    }
}