using KestrelCore;
using SuperSocket.Channel;

namespace KestrelServer.Commands;

public interface IAsyncCommand
{
    CommandType CommandType { get; }

    ValueTask ExecuteAsync(IChannel channel, CommandMessage message);
}