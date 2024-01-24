using Google.Protobuf;
using KestrelCore;
using SuperSocket.Command;

namespace KestrelServer.SSServer;

[Command(Key = (int)CommandType.Login)]
public sealed class LoginCommand : IAsyncCommand<TestSession,CommandMessage>
{
    public async ValueTask ExecuteAsync(TestSession session, CommandMessage package)
    {
        var request = LoginMessageRequest.Parser.ParseFrom(package.Content);
        
        await session.SendMessageAsync(new CommandMessage
        {
            Key = CommandType.LoginReply,
            Content = new LoginMessageReply
            {
                Token = "sssssss"
            }.ToByteString()
        });
    }
}