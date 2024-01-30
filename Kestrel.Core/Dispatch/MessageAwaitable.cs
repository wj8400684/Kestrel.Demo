using Kestrel.Core.Messages;

namespace Kestrel.Core;

// public sealed class MessageAwaitable<TMessage>(ulong packetIdentifier, MessageDispatcher owningMessageDispatcher)
//     : IMessageAwaitable
//     where TMessage : CommandMessage
// {
//     private readonly TaskCompletionSource<CommandMessage> _promise =
//         new(TaskCreationOptions.RunContinuationsAsynchronously);
//
//     public async Task<TMessage> WaitAsync(CancellationToken cancellationToken)
//     {
//         cancellationToken.ThrowIfCancellationRequested();
//
//         await using var register = cancellationToken.Register(() => Fail(new TimeoutException()));
//         return (TMessage)await _promise.Task.ConfigureAwait(false);
//     }
//
//     public void Complete(CommandMessage message)
//     {
//         ArgumentNullException.ThrowIfNull(message);
//
//         _promise.TrySetResult(message);
//     }
//
//     public void Fail(Exception exception)
//     {
//         ArgumentNullException.ThrowIfNull(exception);
//
//         _promise.TrySetException(exception);
//     }
//
//     public void Cancel()
//     {
//         _promise.TrySetCanceled();
//     }
//
//     public void Dispose()
//     {
//         owningMessageDispatcher.RemoveAwaitable(packetIdentifier);
//     }
// }