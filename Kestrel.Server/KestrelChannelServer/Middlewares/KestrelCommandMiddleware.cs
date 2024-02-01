using System.Collections.ObjectModel;
using KestrelCore;
using KestrelServer.Commands;

namespace KestrelServer.Middlewares;

public sealed class KestrelCommandMiddleware(IEnumerable<IKestrelAsyncCommand> commands)
    : IApplicationMiddleware<KestrelCommandContext>
{
    private readonly ReadOnlyDictionary<CommandType, IKestrelAsyncCommand> _commands = new(commands.ToDictionary(
        item => item.CommandType,
        item => item));

    async ValueTask IApplicationMiddleware<KestrelCommandContext>.InvokeAsync(
        ApplicationDelegate<KestrelCommandContext> next,
        KestrelCommandContext context)
    {
        if (_commands.TryGetValue(context.Message.Key, out var command))
            await command.ExecuteAsync(context.Channel, context.Message);
        else
            await next(context);
    }
}