using System.Collections.Concurrent;
using Kestrel.Core.Messages;

namespace Kestrel.Core;

public static class ValueMessageAwaitablePool
{
    public static ValueMessageAwaitablePool<TMessage> Crete<TMessage>()
        where TMessage : CommandMessage
    {
        return new ValueMessageAwaitablePool<TMessage>();
    }
}


public class ValueMessageAwaitablePool<TMessage>
    where TMessage : CommandMessage
{
    private readonly ConcurrentQueue<ValueMessageAwaitable<TMessage>> _messagePool = new();

    public ValueMessageAwaitable<TMessage> Create(ulong packetIdentifier, MessageDispatcher owningMessageDispatcher)
    {
        if (!_messagePool.TryDequeue(out var awaitable))
            return new ValueMessageAwaitable<TMessage>(packetIdentifier, owningMessageDispatcher);
        
        awaitable.Rest(packetIdentifier);
        return awaitable;
    }

    public void Return(ValueMessageAwaitable<TMessage> awaitable)
    {
        _messagePool.Enqueue(awaitable);
    }
}