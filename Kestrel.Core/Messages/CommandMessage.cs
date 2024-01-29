using System.Buffers;
using MemoryPack;
using SuperSocket.ProtoBase;

namespace Kestrel.Core.Messages;

public abstract class CommandMessage : IKeyedPackageInfo<CommandType>
{
    public const byte HeaderSize = sizeof(short);
    
    protected readonly Type Type;
    private static readonly Dictionary<Type, CommandType> CommandTypes = new();

    protected CommandMessage(CommandType key)
    {
        Key = key;
        Type = GetType();
    }

    #region command inilizetion

    internal static void LoadAllCommand()
    {
        var packets = typeof(CommandMessage).Assembly.GetTypes()
            .Where(t => typeof(CommandMessage).IsAssignableFrom(t))
            .Where(t => t is { IsAbstract: false, IsClass: true })
            .Select(t => (CommandMessage?)Activator.CreateInstance(t));

        using var enumerator = packets.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current != null)
                CommandTypes.TryAdd(enumerator.Current.GetType(), enumerator.Current.Key);
        }
    }

    public static CommandType GetCommandKey<TPacket>()
    {
        var type = typeof(TPacket);

        if (!CommandTypes.TryGetValue(type, out var key))
            throw new Exception($"{type.Name} δ�̳�PlayPacket");

        return key;
    }

    public static List<KeyValuePair<Type, CommandType>> GetCommands()
    {
        return CommandTypes.ToList();
    }

    static CommandMessage()
    {
        LoadAllCommand();
    }

    #endregion

    /// <summary>
    /// 命令
    /// </summary>
    [MemoryPackIgnore]
    public CommandType Key { get; }

    public virtual int Encode(IBufferWriter<byte> bufWriter)
    {
        using var state = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializerOptions.Utf8);
        var writer = new MemoryPackWriter<IBufferWriter<byte>>(ref bufWriter, state);
        writer.WriteValue(Type, this);
        var writtenCount = writer.WrittenCount;
        writer.Flush();

        return writtenCount;
    }

    public virtual void DecodeBody(ref SequenceReader<byte> reader, object? context)
    {
        MemoryPackSerializer.Deserialize(Type, reader.UnreadSequence, ref context);
    }

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this, Type);
    }
}

public abstract class CommandRespMessage(CommandType type) : CommandMessage(type)
{
    public string? ErrorMessage { get; set; }

    public bool SuccessFul { get; set; }

    public int ErrorCode { get; set; }
}

public abstract class CommandMessageWithIdentifier(CommandType type) : CommandMessage(type)
{
    public ulong Identifier { get; set; }
}

public abstract class CommandRespMessageWithIdentifier(CommandType type) : CommandMessageWithIdentifier(type)
{
    public string? ErrorMessage { get; set; }

    public bool SuccessFul { get; set; }

    public int ErrorCode { get; set; }
}