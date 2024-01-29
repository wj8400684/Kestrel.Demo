using Google.Protobuf;
using KestrelCore;
using KestrelServer.Server;

namespace KestrelServer.Commands;

public sealed class LoginCommand : RequestAsyncCommand<LoginMessageRequest>
{
    public override CommandType CommandType => CommandType.Login;

    protected override async ValueTask OnHandlerAsync(AppChannel session, 
        CommandMessage message, 
        LoginMessageRequest request,
        CancellationToken cancellationToken)
    {
        var reply = new CommandMessage
        {
            SuccessFul = true,
            Identifier = message.Identifier,
            Content = new LoginMessageReply
            {
                Token = "dddddd"
            }.ToByteString()
        };
        
        await session.WriterAsync(reply, cancellationToken);
    }
}