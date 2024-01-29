using MemoryPack;

namespace Kestrel.Core.Messages;

[MemoryPackable]
public sealed partial class LoginRequestMessage() : CommandMessageWithIdentifier(CommandType.Login)
{
    public string? Username { get; set; }

    public string? Password { get; set; }
}

[MemoryPackable]
public sealed partial class LoginReplyMessage() : CommandRespMessageWithIdentifier(CommandType.LoginReply)
{
}