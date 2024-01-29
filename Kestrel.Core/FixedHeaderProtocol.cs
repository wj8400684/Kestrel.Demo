using System.Buffers;
using System.Buffers.Binary;
using Bedrock.Framework.Protocols;
using Kestrel.Core.Extensions;
using Kestrel.Core.Messages;
using SuperSocket.ProtoBase;

namespace KestrelCore;

public readonly struct FixedHeaderProtocol(IMessageFactoryPool messageFactoryPool) :
    IMessageReader<CommandMessage>,
    IMessageWriter<CommandMessage>
{
    public bool TryParseMessage(in ReadOnlySequence<byte> input,
        ref SequencePosition consumed,
        ref SequencePosition examined,
        out CommandMessage message)
    {
        message = default;

        if (input.IsEmpty)
            return false;

        var reader = new SequenceReader<byte>(input);
        if (!reader.TryReadLittleEndian(out short bodyLength) || reader.Remaining < bodyLength)
            return false;

        reader.TryRead(out var command);

        var packetFactory = messageFactoryPool.Get(command) ?? throw new ProtocolException($"????{command}?????");

        message = packetFactory.Create();

        message.DecodeBody(ref reader, message);
        
        examined = consumed = input.Slice(bodyLength + CommandMessage.HeaderSize).End;
        return true;
    }

    public void WriteMessage(CommandMessage message, IBufferWriter<byte> output)
    {
        var headSpan = output.GetSpan(CommandMessage.HeaderSize);
        output.Advance(CommandMessage.HeaderSize);

        var length = output.WriteLittleEndian((byte)message.Key);

        length += message.Encode(output);

        BinaryPrimitives.WriteInt16LittleEndian(headSpan, (short)length);
    }
}