using System.Collections.Concurrent;
using Kestrel.Core.Messages;

namespace Kestrel.Core;

public sealed class MessageDispatcher : IDisposable
{
    private bool _isDisposed;
    private readonly Dictionary<ulong, IMessageAwaitable> _waiters = new();

    public IMessageAwaitable<TResponseMessage> AddAwaitable<TResponseMessage>(ulong messageIdentifier)
        where TResponseMessage : CommandMessage
    {
        var awaitable = new ValueMessageAwaitable<TResponseMessage>(messageIdentifier, this);

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

                packetAwait.Cancel();
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

                packetAwait.Cancel();
            }

            _waiters.Clear();
        }
    }

    public void RemoveAwaitable(ulong identifier)
    {
        lock (_waiters)
            _waiters.Remove(identifier, out _);
    }

    public bool TryDispatch(CommandRespMessageWithIdentifier message)
    {
        ArgumentNullException.ThrowIfNull(message);

        ThrowIfDisposed();

        IMessageAwaitable? awaitable;

        lock (_waiters)
        {
            if (!_waiters.Remove(message.Identifier, out awaitable))
                return false;
        }

        awaitable.Complete(message);

        return true;
    }

    void ThrowIfDisposed()
    {
        if (!_isDisposed)
            return;
        throw new ObjectDisposedException(nameof(MessageDispatcher));
    }
}