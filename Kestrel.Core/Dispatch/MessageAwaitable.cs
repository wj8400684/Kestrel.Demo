using System.Threading.Tasks.Sources;
using Google.Protobuf;
using KestrelCore;

namespace Kestrel.Core;

public sealed class MessageAwaitable<TMessage>(uint packetIdentifier, MessageDispatcher owningMessageDispatcher)
    : IMessageAwaitable,
        IValueTaskSource<CommandMessage>
    where TMessage : IMessage<TMessage>
{
    private ManualResetValueTaskSourceCore<CommandMessage> _taskSourceCore = new()
    {
        RunContinuationsAsynchronously = true,
    };

    public async Task<ValueCommandResponse<TMessage>> WaitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        //await using var register = cancellationToken.Register(() => Fail(new TimeoutException()));

        var message = await new ValueTask<CommandMessage>(this, _taskSourceCore.Version);
        
        return new ValueCommandResponse<TMessage>(message);
    }

    public void Complete(CommandMessage message)
    {
        _taskSourceCore.SetResult(message);
    }
    
    public void Fail(Exception exception)
    {
        _taskSourceCore.SetException(exception);
    }
    
    public void Cancel()
    {
        _taskSourceCore.SetException(new TaskCanceledException());
    }
    

    public void Dispose()
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