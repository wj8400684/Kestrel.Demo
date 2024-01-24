using System.Buffers;
using System.Buffers.Binary;
using Bedrock.Framework.Protocols;
using Google.Protobuf;

namespace KestrelCore;

public struct FixedHeaderPipelineFilter :
    IMessageReader<CommandMessage>,
    IMessageWriter<CommandMessage>
{
    private const int HeaderSize = sizeof(int);

    public bool TryParseMessage(in ReadOnlySequence<byte> input,
        ref SequencePosition consumed,
        ref SequencePosition examined,
        out CommandMessage message)
    {
        message = default;
        
        if (input.IsEmpty)
            return false;
        
        var reader = new SequenceReader<byte>(input);
        if (!reader.TryReadLittleEndian(out int bodyLength) || reader.Remaining < bodyLength)
            return false;

        var payLoad = reader.Sequence.Slice(HeaderSize, bodyLength);

        message = CommandMessage.Parser.ParseFrom(payLoad);

        examined = consumed = payLoad.End;
        return true;
    }

    public void WriteMessage(CommandMessage message, IBufferWriter<byte> output)
    {
        var bodyLength = message.CalculateSize();

        var headSpan = output.GetSpan(HeaderSize);
        BinaryPrimitives.WriteInt32LittleEndian(headSpan, bodyLength);
        output.Advance(HeaderSize);
        
        message.WriteTo(output);
    }
}