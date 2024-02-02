using KestrelCore;
using SuperSocket.Channel;

namespace KestrelServer.Commands;

public interface IKestrelAsyncCommand
{
    CommandType CommandType { get; }

    ValueTask ExecuteAsync(IChannel channel, CommandMessage message);
}