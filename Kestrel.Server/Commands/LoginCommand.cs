using KestrelCore;
using KestrelServer.Server;

namespace KestrelServer.Commands;

public sealed class LoginCommand : RequestAsyncCommand<LoginMessageRequest>
{
    public override CommandType CommandType => CommandType.Login;

    protected override ValueTask OnHandlerAsync(AppChannel session, CommandMessage package, LoginMessageRequest request,
        CancellationToken cancellationToken)
    {
        return session.WriterAsync(CommandMessage.NewReplyMessage(CommandType.LoginReply, new LoginMessageReply
        {
            Token = "sssss",
        }), cancellationToken);
    }
}