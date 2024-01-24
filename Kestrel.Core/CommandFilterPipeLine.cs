using System.Buffers;
using System.Buffers.Binary;
using SuperSocket.ProtoBase;

namespace KestrelCore;

public class CommandFilterPipeLine() : FixedHeaderPipelineFilter<CommandMessage>(CommandMessage.HeaderSize)
{
    protected override CommandMessage DecodePackage(ref ReadOnlySequence<byte> buffer)
    {
        return CommandMessage.Parser.ParseFrom(buffer.Slice(CommandMessage.HeaderSize));
    }

    protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
    {
        int bodyLength;

        if (buffer.IsSingleSegment)
            BinaryPrimitives.TryReadInt32LittleEndian(buffer.FirstSpan, out bodyLength);
        else
            BinaryPrimitives.TryReadInt32LittleEndian(buffer.ToArray(), out bodyLength);

        return bodyLength;
    }
}