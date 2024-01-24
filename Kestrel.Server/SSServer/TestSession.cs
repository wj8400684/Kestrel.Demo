using KestrelCore;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace KestrelServer.SSServer;

public sealed class TestSession(IPackageEncoder<CommandMessage> encoder) : AppSession
{
    public ValueTask SendMessageAsync(CommandMessage message)
    {
        return Channel.IsClosed ? ValueTask.CompletedTask : Channel.SendAsync(encoder, message);
    }
}