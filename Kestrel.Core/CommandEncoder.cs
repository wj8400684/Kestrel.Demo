using System.Buffers;
using System.Buffers.Binary;
using Google.Protobuf;
using KestrelCore;
using SuperSocket.ProtoBase;

namespace KestrelCore;

public class CommandEncoder : IPackageEncoder<CommandMessage>
{
    public int Encode(IBufferWriter<byte> writer, CommandMessage pack)
    {
        var bodyLength = pack.CalculateSize();

        var headSpan = writer.GetSpan(CommandMessage.HeaderSize);
        BinaryPrimitives.WriteInt32LittleEndian(headSpan, bodyLength);
        writer.Advance(CommandMessage.HeaderSize);
        
        pack.WriteTo(writer);

        return bodyLength + CommandMessage.HeaderSize;
    }
}