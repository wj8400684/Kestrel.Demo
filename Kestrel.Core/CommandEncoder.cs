using System.Buffers;
using System.Buffers.Binary;
using Kestrel.Core.Extensions;
using Kestrel.Core.Messages;
using SuperSocket.ProtoBase;

namespace KestrelCore;

public class CommandEncoder : IPackageEncoder<CommandMessage>
{
    public int Encode(IBufferWriter<byte> writer, CommandMessage pack)
    {
        var headSpan = writer.GetSpan(CommandMessage.HeaderSize);
        writer.Advance(CommandMessage.HeaderSize);

        var length = writer.WriteLittleEndian((byte)pack.Key);

        length += pack.Encode(writer);

        BinaryPrimitives.WriteInt16LittleEndian(headSpan, (short)length);

        return CommandMessage.HeaderSize + length;
    }
}