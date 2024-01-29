using Kestrel.Core.Messages;
using KestrelServer.Commands;
using KestrelServer.Server;

namespace Kestrel.Server.Commands;

public abstract class RequestAsyncCommand<TRequest, TResponse> : IAsyncCommand
    where TRequest : CommandMessageWithIdentifier
    where TResponse : CommandRespMessageWithIdentifier, new()
{
    public abstract CommandType CommandType { get; }

    ValueTask IAsyncCommand.ExecuteAsync(AppChannel channel, CommandMessage message)
    {
        return SchedulerAsync(channel, message, CancellationToken.None);
    }

    protected virtual TResponse CreateRespMessage(TRequest request, bool successFul = false)
    {
        return new TResponse()
        {
            SuccessFul = successFul,
            Identifier = request.Identifier,
        };
    }

    protected virtual async ValueTask SchedulerAsync(AppChannel session, CommandMessage message,
        CancellationToken cancellationToken)
    {
        TResponse respMessage;
        var request = (TRequest)message;

        try
        {
            respMessage = await ExecuteAsync(session, request, cancellationToken);
        }
        catch (Exception e)
        {
            respMessage = CreateRespMessage(request);
            respMessage.ErrorMessage = "未知错误请稍后重试";
            session.LogError(e, $"{session.RemoteEndPoint}-{message.Key} 抛出一个未知异常");
        }

        await session.WriterAsync(respMessage, cancellationToken);
    }

    protected abstract ValueTask<TResponse> ExecuteAsync(AppChannel session, TRequest request,
        CancellationToken cancellationToken);
}