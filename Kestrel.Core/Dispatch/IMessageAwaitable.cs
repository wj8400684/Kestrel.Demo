using KestrelCore;

namespace Kestrel.Core;

public interface IMessageAwaitable : IDisposable
{
    void Complete(CommandMessage message);

    void Fail(Exception exception);

    void Cancel();


    void CompleteForValueTask(CommandMessage message);

    void FailForValueTask(Exception exception);

    void CancelForValueTask();
}