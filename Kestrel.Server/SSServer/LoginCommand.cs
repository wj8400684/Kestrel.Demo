using Kestrel.Core.Messages;
using SuperSocket.Command;

namespace KestrelServer.SSServer;

[Command(Key = (byte)CommandType.Login)]
public sealed class LoginCommand : RequestAsyncCommand<LoginRequestMessage, LoginReplyMessage>
{
    protected override ValueTask<LoginReplyMessage> ExecuteAsync(
        TestSession session, 
        LoginRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var response = CreateRespMessage(request);

        return new ValueTask<LoginReplyMessage>(response);
    }
}