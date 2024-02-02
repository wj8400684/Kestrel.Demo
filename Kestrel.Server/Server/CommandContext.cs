using KestrelCore;
using SuperSocket.Channel;

namespace KestrelServer;

public readonly struct CommandContext
{
    public required IChannel Channel { get; init; }

    public required CommandMessage Message { get; init; }
}