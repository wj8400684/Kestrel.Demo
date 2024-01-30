using Kestrel.Core.Messages;
using KestrelCore;

namespace Kestrel.Core;

public interface IMessageAwaitable<TMessage> : IMessageAwaitable
{
    ValueTask<TMessage> WaitAsync(CancellationToken cancellationToken);
}

public interface IMessageAwaitable : IDisposable
{
    void Complete(CommandMessage message);

    void Fail(Exception exception);

    void Cancel();
}