using System.Collections.ObjectModel;
using KestrelCore;
using KestrelServer.Commands;

namespace KestrelServer.Middlewares;

public sealed class CommandMiddleware
    (IEnumerable<IAsyncCommand> commands) : IApplicationMiddleware<CommandContext>
{
    private readonly ReadOnlyDictionary<CommandType, IAsyncCommand> _commands = new(commands.ToDictionary(
        item => item.CommandType,
        item => item));

    async ValueTask IApplicationMiddleware<CommandContext>.InvokeAsync(ApplicationDelegate<CommandContext> next,
        CommandContext context)
    {
        if (_commands.TryGetValue(context.Message.Key, out var command))
            await command.ExecuteAsync(context.Channel, context.Message);
        else
            await next(context);
    }
}