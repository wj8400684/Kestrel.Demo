using Google.Protobuf;
using KestrelCore;

namespace Kestrel.Core;

public sealed class MessageDispatcher : IDisposable
{
    private bool _isDisposed;
    private readonly Dictionary<uint, IMessageAwaitable> _waiters = new();

    public MessageAwaitable<TResponseMessage> AddAwaitable<TResponseMessage>(uint messageIdentifier)
        where TResponseMessage : IMessage<TResponseMessage>
    {
        var awaitable = new MessageAwaitable<TResponseMessage>(messageIdentifier, this);

        lock (_waiters)
            _waiters.TryAdd(messageIdentifier, awaitable);

        return awaitable;
    }

    public void CancelAll()
    {
        lock (_waiters)
        {
            using var enumerator = _waiters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var packetAwait = enumerator.Current.Value;

                packetAwait.CancelForValueTask();
            }

            _waiters.Clear();
        }
    }

    public void Dispose()
    {
        Dispose(new ObjectDisposedException(nameof(MessageDispatcher)));
    }

    public void Dispose(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        lock (_waiters)
        {
            FailAll(exception);

            // Make sure that no task can start waiting after this instance is already disposed.
            // This will prevent unexpected freezes.
            _isDisposed = true;
        }
    }

    public void FailAll(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        lock (_waiters)
        {
            using var enumerator = _waiters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var packetAwait = enumerator.Current.Value;

                packetAwait.FailForValueTask(exception);
            }

            _waiters.Clear();
        }
    }

    public void RemoveAwaitable(uint identifier)
    {
        lock (_waiters)
            _waiters.Remove(identifier);
    }

    public bool TryDispatch(CommandMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        IMessageAwaitable? awaitable;

        lock (_waiters)
        {
            ThrowIfDisposed();

            if (!_waiters.Remove(message.Identifier, out awaitable))
                return false;
        }

        awaitable.CompleteForValueTask(message);

        return true;
    }

    void ThrowIfDisposed()
    {
        if (!_isDisposed) 
            return;
        throw new ObjectDisposedException(nameof(MessageDispatcher));
    }
}