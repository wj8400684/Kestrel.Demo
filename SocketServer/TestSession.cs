using Kestrel.Core.Messages;
using KestrelCore;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SocketServer;

public sealed class TestSession(IPackageEncoder<CommandMessage> encoder) : AppSession
{
    public ValueTask SendMessageAsync(CommandMessage message)
    {
        return Channel.IsClosed ? ValueTask.CompletedTask : Channel.SendAsync(encoder, message);
    }
    
    protected override ValueTask OnSessionConnectedAsync()
    {
        Logger.LogInformation($"OnSessionConnectedAsync-RemoteEndPoint:{RemoteEndPoint}-LocalEndPoint:{LocalEndPoint}");
        return base.OnSessionConnectedAsync();
    }

    protected override ValueTask OnSessionClosedAsync(CloseEventArgs e)
    {
        Logger.LogInformation($"{nameof(OnSessionClosedAsync)}-RemoteEndPoint:{RemoteEndPoint}-LocalEndPoint:{LocalEndPoint}");
        return base.OnSessionClosedAsync(e);
    }
}