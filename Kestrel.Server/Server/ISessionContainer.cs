using KestrelServer.Server;

namespace Kestrel.Server.Server;

public interface ISessionContainer
{
    ValueTask<bool> RegisterSessionAsync(AppChannel session);

    ValueTask<bool> UnRegisterSessionAsync(AppChannel session);

    AppChannel? GetSessionById(string connectionId);

    int GetSessionCount();

    IEnumerable<AppChannel> GetSessions(Predicate<AppChannel>? criteria = null);
}