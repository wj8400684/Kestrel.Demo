namespace Kestrel.Core.Messages;

public interface IMessageFactoryPool
{
    IMessageFactory Get(CommandType type);

    IMessageFactory? Get(byte command);
}

public class CommandMessageFactoryPool : IMessageFactoryPool
{
    private readonly IMessageFactory[] _messageFactories;

    public CommandMessageFactoryPool()
    {
        _messageFactories = Inilizetion();
    }

    protected virtual IMessageFactory[] Inilizetion()
    {
        var commands = CommandMessage.GetCommands();

        var messageFactories = new IMessageFactory[commands.Count + 1];

        foreach (var command in commands)
        {
            var genericType = typeof(DefaultMessageFactory<>).MakeGenericType(command.Key);

            if (Activator.CreateInstance(genericType) is not IMessageFactory messageFactory)
                continue;

            messageFactories[(int)command.Value] = messageFactory;
        }

        return messageFactories;
    }

    public IMessageFactory Get(CommandType command)
    {
        return Get((byte)command)!;
    }

    public IMessageFactory? Get(byte command)
    {
        return command > _messageFactories.Length ? null : _messageFactories[command];
    }
}