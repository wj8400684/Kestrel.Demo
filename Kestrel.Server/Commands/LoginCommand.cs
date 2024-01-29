using Kestrel.Core.Messages;
using Kestrel.Server.Commands;
using KestrelServer.Server;

namespace KestrelServer.Commands;

public sealed class LoginCommand : RequestAsyncCommand<LoginRequestMessage, LoginReplyMessage>
{
    public override CommandType CommandType => CommandType.Login;

    protected override ValueTask<LoginReplyMessage> ExecuteAsync(
        AppChannel session,
        LoginRequestMessage request,
        CancellationToken cancellationToken)
    {
        var reply = CreateRespMessage(request);

        return new ValueTask<LoginReplyMessage>(reply);
    }
}