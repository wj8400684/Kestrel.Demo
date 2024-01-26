using System.Collections.Concurrent;
using KestrelServer.Middlewares;
using KestrelServer.Server;
using Microsoft.AspNetCore.Connections;

namespace KestrelServer;

public interface ISessionContainer
{
    ValueTask<bool> RegisterSessionAsync(AppChannel session);

    ValueTask<bool> UnRegisterSessionAsync(AppChannel session);

    AppChannel? GetSessionById(string connectionId);

    int GetSessionCount();

    IEnumerable<AppChannel> GetSessions(Predicate<AppChannel>? criteria = null);
}

public sealed class InProcSessionContainer : ISessionContainer
{
    private readonly ConcurrentDictionary<string, AppChannel> _sessions = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask<bool> RegisterSessionAsync(AppChannel session)
    {
        _sessions.TryAdd(session.ConnectionId, session);
        return new ValueTask<bool>(true);
    }

    public ValueTask<bool> UnRegisterSessionAsync(AppChannel session)
    {
        _sessions.TryRemove(session.ConnectionId, out _);
        return new ValueTask<bool>(true);
    }

    public AppChannel? GetSessionById(string connectionId)
    {
        _sessions.TryGetValue(connectionId, out var session);

        return session;
    }

    public int GetSessionCount()
    {
        return _sessions.Count;
    }

    public IEnumerable<AppChannel> GetSessions(Predicate<AppChannel>? criteria = null)
    {
        using var enumerator = _sessions.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var s = enumerator.Current.Value;

            if (criteria == null || criteria(s))
                yield return s;
        }
    }
}

public sealed class CommandConnectionHandler(
    ILogger<CommandConnectionHandler> logger,
    IServiceProvider appServices) : ConnectionHandler
{
    private readonly ISessionContainer _sessionContainer = appServices.GetRequiredService<ISessionContainer>();

    private readonly ApplicationDelegate<CommandContext> _application =
        new ApplicationBuilder<CommandContext>(appServices)
            .Use<AuthorMiddleware>()
            .Use<CommandMiddleware>()
            .Build();

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        await using var channel = new AppChannel(connection, logger);

        await _sessionContainer.RegisterSessionAsync(channel);

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
            logger.LogError(e, $"The session disconnected: {connection.ConnectionId}");
            return;
        }
        finally
        {
            await _sessionContainer.UnRegisterSessionAsync(channel);
        }

        logger.LogInformation($"The session disconnected: {connection.ConnectionId}");
    }
}