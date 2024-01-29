using System.Buffers;
using System.Buffers.Binary;
using Kestrel.Core.Messages;
using SuperSocket.ProtoBase;

namespace KestrelCore;

public class CommandFilterPipeLine() : FixedHeaderPipelineFilter<CommandMessage>(CommandMessage.HeaderSize)
{
    protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
    {
        short bodyLength;

        if (buffer.IsSingleSegment)
            BinaryPrimitives.TryReadInt16LittleEndian(buffer.FirstSpan, out bodyLength);
        else
        {
            var reader = new SequenceReader<byte>(buffer);
            reader.TryReadLittleEndian(out bodyLength);
        }
        
        return bodyLength;
    }
}