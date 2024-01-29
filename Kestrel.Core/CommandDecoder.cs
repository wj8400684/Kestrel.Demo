using System.Buffers;
using Kestrel.Core.Messages;
using SuperSocket.ProtoBase;

namespace Kestrel.Core;

public class CommandDecoder(IMessageFactoryPool messageFactoryPool) : IPackageDecoder<CommandMessage>
{
    public CommandMessage Decode(ref ReadOnlySequence<byte> buffer, object context)
    {
        var reader = new SequenceReader<byte>(buffer);

        reader.Advance(CommandMessage.HeaderSize);

        reader.TryRead(out var command);

        var messageFactory = messageFactoryPool.Get(command) ?? throw new ProtocolException($"????{command}?????");

        var message = messageFactory.Create();

        message.DecodeBody(ref reader, message);

        return message;
    }
}