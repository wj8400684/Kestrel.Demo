using System.Collections.Concurrent;
using KestrelServer.Server;

namespace Kestrel.Server.Server;

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