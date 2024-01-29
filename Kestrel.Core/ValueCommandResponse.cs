using System.Collections.Concurrent;
using Google.Protobuf;
using KestrelCore;

namespace Kestrel.Core;

public readonly struct ValueCommandResponse<TMessage>(in CommandMessage message)
    where TMessage : IMessage<TMessage>
{
    private readonly CommandMessage _message = message;
    private static readonly ConcurrentDictionary<Type, MessageParser<TMessage>> MessageParserCache = new();

    public TMessage DecodeMessage()
    {
        var messageParser = MessageParserCache.GetOrAdd(
            key: typeof(TMessage),
            valueFactory: _ => new MessageParser<TMessage>(Activator.CreateInstance<TMessage>));

        return messageParser.ParseFrom(_message.Content);
    }

    public bool SuccessFul { get; } = message.SuccessFul;

    public ErrorCode ErrorCode { get; } = message.ErrorCode;

    public string? ErrorMessage { get; } = message.ErrorMessage;
}