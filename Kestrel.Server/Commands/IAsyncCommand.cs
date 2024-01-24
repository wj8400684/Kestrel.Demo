using KestrelCore;
using KestrelServer.Server;

namespace KestrelServer.Commands;

public interface IAsyncCommand
{
    CommandType CommandType { get; }

    ValueTask ExecuteAsync(AppChannel channel, CommandMessage message);
}
