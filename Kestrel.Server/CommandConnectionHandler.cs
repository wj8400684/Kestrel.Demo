using KestrelServer.Middlewares;
using KestrelServer.Server;
using Microsoft.AspNetCore.Connections;

namespace KestrelServer;

public sealed class CommandConnectionHandler(
    ILogger<CommandConnectionHandler> logger,
    IServiceProvider appServices) : ConnectionHandler
{
    private readonly ApplicationDelegate<CommandContext> _application =
        new ApplicationBuilder<CommandContext>(appServices)
            .Use<AuthorMiddleware>()
            .Use<CommandMiddleware>()
            .Build();

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        await using var channel = new AppChannel(connection, logger);

        logger.LogInformation($"A new session connected: {connection.ConnectionId}");
        
        try
        {
            while (!connection.ConnectionClosed.IsCancellationRequested)
            {
                var message = await channel.ReadAsync();

                if (message == null)
                    continue;

                await _application(new CommandContext
                {
                    Channel = channel,
                    Message = message,
                });
            }
        }
        catch (Exception e)
        {
            logger.LogError(e,$"The session disconnected: {connection.ConnectionId}");
            return;
        }

        logger.LogInformation($"The session disconnected: {connection.ConnectionId}");
    }
}