using KestrelCore;
using KestrelServer.Server;

namespace KestrelServer;

public readonly struct CommandContext
{
    public required AppChannel Channel { get; init; }

    public required CommandMessage Message { get; init; }
}