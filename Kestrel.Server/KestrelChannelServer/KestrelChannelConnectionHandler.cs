using System.Collections.Concurrent;
using Kestrel.Core;
using KestrelCore;
using KestrelServer.Commands;
using KestrelServer.Middlewares;
using KestrelServer.Server;
using Microsoft.AspNetCore.Connections;
using SuperSocket.Channel;
using SuperSocket.Channel.Kestrel;

namespace KestrelServer;

public sealed class KestrelChannelConnectionHandler(
    ILogger<CommandConnectionHandler> logger,
    IServiceProvider appServices) : ConnectionHandler
{
    private readonly ApplicationDelegate<KestrelCommandContext> _application =
        new ApplicationBuilder<KestrelCommandContext>(appServices)
            .Use<KestrelCommandMiddleware>()
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
                await _application(new KestrelCommandContext
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