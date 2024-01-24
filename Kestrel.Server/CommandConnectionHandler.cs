using KestrelServer.Middlewares;
using KestrelServer.Server;
using Microsoft.AspNetCore.Connections;

namespace KestrelServer;

public sealed class CommandConnectionHandler(
    ILogger<CommandConnectionHandler> logger,
    IServiceProvider appServices) : ConnectionHandler
{
    private readonly ApplicationDelegate<CommandContext> _application = new ApplicationBuilder<CommandContext>(appServices)
        .Use<AuthorMiddleware>()
        .Use<CommandMiddleware>()
        .Build();

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        logger.LogInformation($"客户端连接：{connection.ConnectionId}-{connection.RemoteEndPoint}");

        await using (var channel = new AppChannel(connection))
        {
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
                logger.LogError(e, $"断开连接：{connection.ConnectionId}-{connection.RemoteEndPoint}");
                return;
            }
        }

        logger.LogWarning($"断开连接：{connection.ConnectionId}-{connection.RemoteEndPoint}");
    }
}