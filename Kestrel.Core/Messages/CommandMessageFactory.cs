namespace Kestrel.Core.Messages;

public interface IMessageFactory
{
    CommandMessage Create();

    void Return(CommandMessage package);
}

public sealed class DefaultMessageFactory<TMessage> : IMessageFactory
    where TMessage : CommandMessage, new()
{
    public CommandMessage Create()
    {
        return new TMessage();
    }

    public void Return(CommandMessage message)
    {
    }
}