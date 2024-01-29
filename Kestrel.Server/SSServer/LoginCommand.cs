using Kestrel.Core.Messages;
using SuperSocket;
using SuperSocket.Command;

namespace KestrelServer.SSServer;

[Command(Key = (byte)CommandType.Login)]
public sealed class LoginCommand : IAsyncCommand<LoginRequestMessage>
{
    public ValueTask ExecuteAsync(IAppSession session, LoginRequestMessage package)
    {
        var s = (TestSession)session;
        
        return s.SendMessageAsync(new LoginReplyMessage
        {
            Identifier = package.Identifier,
        });
    }
}