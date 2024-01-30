using System.Threading.Tasks.Sources;
using Kestrel.Core.Messages;

namespace Kestrel.Core;

public struct ValueMessageAwaitable<TMessage>(ulong packetIdentifier, MessageDispatcher owningMessageDispatcher)
    : IMessageAwaitable<TMessage>, IValueTaskSource<TMessage>
    where TMessage : CommandMessage
{
    private ManualResetValueTaskSourceCore<TMessage> _taskSourceCore =
        new()
        {
            RunContinuationsAsynchronously = true
        };

    public ValueTask<TMessage> WaitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        //await using var register = cancellationToken.Register(() => Fail(new TimeoutException()));
        return new ValueTask<TMessage>(this, _taskSourceCore.Version);
    }

    public void Complete(CommandMessage message)
    {
        _taskSourceCore.SetResult((TMessage)message);
    }

    public void Fail(Exception exception)
    {
        _taskSourceCore.SetException(exception);
    }

    public void Cancel()
    {
        _taskSourceCore.SetException(new TaskCanceledException());
    }

    void IDisposable.Dispose()
    {
        owningMessageDispatcher.RemoveAwaitable(packetIdentifier);
    }

    TMessage IValueTaskSource<TMessage>.GetResult(short token)
    {
        return _taskSourceCore.GetResult(token);
    }

    ValueTaskSourceStatus IValueTaskSource<TMessage>.GetStatus(short token)
    {
        return _taskSourceCore.GetStatus(token);
    }

    void IValueTaskSource<TMessage>.OnCompleted(Action<object?> continuation, object? state, short token,
        ValueTaskSourceOnCompletedFlags flags)
    {
        _taskSourceCore.OnCompleted(continuation, state, token, flags);
    }
}