using Google.Protobuf;
using KestrelCore;
using SuperSocket.Channel;

namespace KestrelServer.Commands;

public class LoginCommand : RequestAsyncCommand<LoginMessageRequest>
{
    private readonly CommandEncoder _commandEncoder = new();
    
    public override CommandType CommandType => CommandType.Login;

    protected override async ValueTask OnHandlerAsync(IChannel session, 
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
        
        await session.SendAsync(_commandEncoder, reply);
    }
}