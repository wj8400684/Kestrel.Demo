using Kestrel.Core.Messages;
using SuperSocket.Command;

namespace KestrelServer.SSServer;

public abstract class RequestAsyncCommand<TRequest> : IAsyncCommand<TestSession, TRequest>
    where TRequest : CommandMessageWithIdentifier
{
    ValueTask IAsyncCommand<TestSession, TRequest>.ExecuteAsync(TestSession session, TRequest package)
    {
        return SchedulerAsync(session, package, CancellationToken.None);
    }

    protected virtual async ValueTask SchedulerAsync(TestSession session, CommandMessage message,
        CancellationToken cancellationToken)
    {
        var request = (TRequest)message;

        try
        {
            await ExecuteAsync(session, request, cancellationToken);
        }
        catch (Exception e)
        {
            session.LogError(e, $"{session.RemoteEndPoint}-{message.Key} 抛出一个未知异常");
        }
    }

    protected abstract ValueTask ExecuteAsync(TestSession session, TRequest request,
        CancellationToken cancellationToken);
}

public abstract class RequestAsyncCommand<TRequest, TResponse> : IAsyncCommand<TestSession, TRequest>
    where TRequest : CommandMessageWithIdentifier
    where TResponse : CommandRespMessageWithIdentifier, new()
{
    ValueTask IAsyncCommand<TestSession, TRequest>.ExecuteAsync(TestSession session, TRequest package)
    {
        return SchedulerAsync(session, package, CancellationToken.None);
    }

    protected virtual TResponse CreateRespMessage(TRequest request, bool successFul = false)
    {
        return new TResponse()
        {
            SuccessFul = successFul,
            Identifier = request.Identifier,
        };
    }

    protected virtual async ValueTask SchedulerAsync(TestSession session, CommandMessage message,
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

        await session.SendMessageAsync(respMessage);
    }

    protected abstract ValueTask<TResponse> ExecuteAsync(TestSession session, TRequest request,
        CancellationToken cancellationToken);
}