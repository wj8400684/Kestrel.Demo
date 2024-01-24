using Google.Protobuf;
using SuperSocket.ProtoBase;

namespace KestrelCore;

public sealed partial class CommandMessage : IKeyedPackageInfo<CommandType>
{
    public const int HeaderSize = sizeof(int);
    
    public static CommandMessage NewMessage<TContent>(CommandType commandType,
        TContent content)
        where TContent : IMessage
    {
        return NewMessage(commandType, content.ToByteString());
    }

    public static CommandMessage NewMessage(CommandType commandType,
        ByteString content)
    {
        return new CommandMessage
        {
            Key = commandType,
            Content = content,
        };
    }
    
    public static CommandMessage NewReplyMessage<TContent>(CommandType commandType,
        TContent content)
        where TContent : IMessage
    {
        return NewReplyMessage(commandType, content.ToByteString());
    }

    public static CommandMessage NewReplyMessage(CommandType commandType,
        ByteString content)
    {
        return new CommandMessage
        {
            Key = commandType,
            SuccessFul = true,
            Content = content,
        };
    }
}