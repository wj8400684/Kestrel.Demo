namespace Kestrel.Core.Messages;

public abstract partial class CommandMessage 
{
    static CommandMessage()
    {
        LoadAllCommand();
    }
    
    public static CommandType GetCommandKey<TMessage>()
    {
        var type = typeof(TMessage);

        if (!CommandTypes.TryGetValue(type, out var key))
            throw new Exception($"{type.Name} δ�̳�PlayPacket");

        return key;
    }

    public static List<KeyValuePair<Type, CommandType>> GetCommands()
    {
        return CommandTypes.ToList();
    }

    private static void LoadAllCommand()
    {
        var messages = typeof(CommandMessage).Assembly.GetTypes()
            .Where(t => typeof(CommandMessage).IsAssignableFrom(t))
            .Where(t => t is { IsAbstract: false, IsClass: true })
            .Select(t => (CommandMessage?)Activator.CreateInstance(t));

        using var enumerator = messages.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current != null)
                CommandTypes.TryAdd(enumerator.Current.GetType(), enumerator.Current.Key);
        }
    }

}