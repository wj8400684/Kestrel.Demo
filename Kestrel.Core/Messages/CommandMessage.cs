using System.Buffers;
using MemoryPack;
using SuperSocket.ProtoBase;

namespace Kestrel.Core.Messages;

public abstract partial class CommandMessage : IKeyedPackageInfo<CommandType>
{
    public const byte HeaderSize = sizeof(short);
    
    protected readonly Type Type;
    private static readonly Dictionary<Type, CommandType> CommandTypes = new();

    protected CommandMessage(CommandType key)
    {
        Key = key;
        Type = GetType();
    }

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