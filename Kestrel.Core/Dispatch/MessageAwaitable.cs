using System.Threading.Tasks.Sources;
using Google.Protobuf;
using KestrelCore;

namespace Kestrel.Core;

public sealed class MessageAwaitable<TMessage>(uint packetIdentifier, MessageDispatcher owningMessageDispatcher)
    : IMessageAwaitable
    where TMessage : IMessage<TMessage>
{
    //private ManualResetValueTaskSourceCore<CommandMessage> _taskSourceCore = new();

    private readonly TaskCompletionSource<CommandMessage> _promise =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task<ValueCommandResponse<TMessage>> WaitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        //await using var register = cancellationToken.Register(() => Fail(new TimeoutException()));
        var message = await _promise.Task.ConfigureAwait(false);

        return new ValueCommandResponse<TMessage>(message);
    }

    // #region ValueTask
    //
    // public async ValueTask<ValueCommandResponse<TMessage>> WaitForValueTaskAsync(CancellationToken cancellationToken)
    // {
    //     cancellationToken.ThrowIfCancellationRequested();
    //
    //     _taskSourceCore.Reset();
    //     
    //     await using var register = cancellationToken.Register(() => Fail(new TimeoutException()));
    //
    //     var message = await new ValueTask<CommandMessage>(this, _taskSourceCore.Version);
    //
    //     return new ValueCommandResponse<TMessage>(message);
    // }
    //
    // public void CompleteForValueTask(CommandMessage message)
    // {
    //     ArgumentNullException.ThrowIfNull(message);
    //
    //     _taskSourceCore.SetResult(message);
    // }
    //
    // public void FailForValueTask(Exception exception)
    // {
    //     ArgumentNullException.ThrowIfNull(exception);
    //
    //     _taskSourceCore.SetException(exception);
    // }
    //
    // public void CancelForValueTask()
    // {
    //     _taskSourceCore.Reset();
    // }
    //
    // #endregion

    public void Complete(CommandMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _promise.TrySetResult(message);
    }

    public void Fail(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _promise.TrySetException(exception);
    }

    public void Cancel()
    {
        _promise.TrySetCanceled();
    }

    public void Dispose()
    {
        owningMessageDispatcher.RemoveAwaitable(packetIdentifier);
    }
    //
    // CommandMessage IValueTaskSource<CommandMessage>.GetResult(short token)
    // {
    //     return _taskSourceCore.GetResult(token);
    // }
    //
    // ValueTaskSourceStatus IValueTaskSource<CommandMessage>.GetStatus(short token)
    // {
    //     return _taskSourceCore.GetStatus(token);
    // }
    //
    // void IValueTaskSource<CommandMessage>.OnCompleted(Action<object?> continuation, object? state, short token,
    //     ValueTaskSourceOnCompletedFlags flags)
    // {
    //     _taskSourceCore.OnCompleted(continuation, state, token, flags);
    // }
}