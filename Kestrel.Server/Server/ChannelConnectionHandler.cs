using KestrelCore;
using KestrelServer.Middlewares;
using Microsoft.AspNetCore.Connections;
using SuperSocket.Channel;
using SuperSocket.Channel.Kestrel;

namespace KestrelServer;

public sealed class ChannelConnectionHandler(
    ILogger<ChannelConnectionHandler> logger,
    IServiceProvider appServices) : ConnectionHandler
{
    private readonly ApplicationDelegate<CommandContext> _application =
        new ApplicationBuilder<CommandContext>(appServices)
            .Use<CommandMiddleware>()
            .Build();

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        logger.LogInformation($"A new session connected: {connection.ConnectionId}");
        
        var channel =
            new KestrelPipeChannel<CommandMessage>(connection, new CommandFilterPipeLine(), new ChannelOptions());

        channel.Start();

        try
        {
            await foreach (var message in channel.RunAsync())
            {
                await _application(new CommandContext
                {
                    Channel = channel,
                    Message = message,
                });
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"The session disconnected: {connection.ConnectionId}");
        }
        finally
        {
            await connection.DisposeAsync();
        }

        logger.LogInformation($"The session disconnected: {connection.ConnectionId}");
    }
}