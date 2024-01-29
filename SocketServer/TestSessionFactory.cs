using Kestrel.Core.Messages;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace SocketServer;

public sealed class TestSessionFactory(IPackageEncoder<CommandMessage> encoder) : ISessionFactory
{
    public Type SessionType => typeof(TestSession);
    
    public IAppSession Create()
    {
        return new TestSession(encoder);
    }
}