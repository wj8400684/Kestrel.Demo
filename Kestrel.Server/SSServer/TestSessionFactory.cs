using KestrelCore;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace KestrelServer.SSServer;

public sealed class TestSessionFactory(IPackageEncoder<CommandMessage> encoder) : ISessionFactory
{
    public Type SessionType => typeof(TestSession);
    
    public IAppSession Create()
    {
        return new TestSession(encoder);
    }
}