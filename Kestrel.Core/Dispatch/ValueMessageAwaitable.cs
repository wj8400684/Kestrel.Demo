using System.Threading.Tasks.Sources;
using Kestrel.Core.Messages;

namespace Kestrel.Core;

public struct ValueMessageAwaitable<TMessage>(ulong packetIdentifier, MessageDispatcher owningMessageDispatcher)
    : IMessageAwaitable<TMessage>, IValueTaskSource<CommandMessage>
    where TMessage : CommandMessage
{
    private ManualResetValueTaskSourceCore<CommandMessage> _taskSourceCore =
        new();

    public async ValueTask<TMessage> WaitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        //await using var register = cancellationToken.Register(() => Fail(new TimeoutException()));
        return (TMessage)await new ValueTask<CommandMessage>(this, _taskSourceCore.Version);
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

    CommandMessage IValueTaskSource<CommandMessage>.GetResult(short token)
    {
        return _taskSourceCore.GetResult(token);
    }

    ValueTaskSourceStatus IValueTaskSource<CommandMessage>.GetStatus(short token)
    {
        return _taskSourceCore.GetStatus(token);
    }

    void IValueTaskSource<CommandMessage>.OnCompleted(Action<object?> continuation, object? state, short token,
        ValueTaskSourceOnCompletedFlags flags)
    {
        _taskSourceCore.OnCompleted(continuation, state, token, flags);
    }
}