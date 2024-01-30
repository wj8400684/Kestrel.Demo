using Bedrock.Framework.Protocols;
using Google.Protobuf;
using Kestrel.Core;
using KestrelCore;
using Microsoft.AspNetCore.Connections;

namespace KestrelServer.Server;

public sealed class AppChannel(ConnectionContext connection, ILogger logger)
    : IAsyncDisposable, ILogger
{
    private readonly MessageDispatcher _messageDispatcher = new();
    private readonly MessageIdentifierProvider _messageIdentifierProvider = new();
    private readonly FixedHeaderPipelineFilter _pipelineFilter = new();
    private readonly ProtocolReader _reader = connection.CreateReader();
    private readonly ProtocolWriter _writer = connection.CreateWriter();

    public bool IsLogin { get; set; }

    public string ConnectionId => connection.ConnectionId;

    public System.Net.EndPoint? RemoteEndPoint => connection.RemoteEndPoint;

    public System.Net.EndPoint? LocalEndPoint => connection.LocalEndPoint;

    public async ValueTask<CommandMessage?> ReadAsync(CancellationToken cancellationToken = default)
    {
        ProtocolReadResult<CommandMessage> result;

        try
        {
            result = await _reader.ReadAsync(_pipelineFilter, cancellationToken);
        }
        finally
        {
            _reader.Advance();
        }

        return result.Message;
    }

    public ValueTask WriterAsync(CommandMessage message,
        CancellationToken cancellationToken = default)
    {
        return _writer.WriteAsync(_pipelineFilter, message, cancellationToken);
    }

    #region dispatch

    /// <summary>
    /// 获取响应包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="responseTimeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal ValueTask<ValueCommandResponse<TReplyMessage>> GetResponsePacketAsync<TReplyMessage>(
        CommandMessage message,
        TimeSpan responseTimeout,
        CancellationToken cancellationToken)
        where TReplyMessage : IMessage<TReplyMessage>
    {
        using var timeOut = new CancellationTokenSource(responseTimeout);
        return GetResponsePacketAsync<TReplyMessage>(message, connection.ConnectionClosed, cancellationToken,
            timeOut.Token);
    }

    /// <summary>
    /// 获取响应包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal ValueTask<ValueCommandResponse<TReplyMessage>> GetResponsePacketAsync<TReplyMessage>(
        CommandMessage message,
        CancellationToken cancellationToken)
        where TReplyMessage : IMessage<TReplyMessage>
    {
        return GetResponsePacketAsync<TReplyMessage>(message, connection.ConnectionClosed, cancellationToken);
    }

    /// <summary>
    /// 获取响应包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="tokens"></param>
    /// <returns></returns>
    internal async ValueTask<ValueCommandResponse<TReplyMessage>> GetResponsePacketAsync<TReplyMessage>(
        CommandMessage message,
        params CancellationToken[] tokens) where TReplyMessage : IMessage<TReplyMessage>
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokens);

        message.Identifier = _messageIdentifierProvider.GetNextIdentifier();

        using var messageAwaitable = _messageDispatcher.AddAwaitable<TReplyMessage>(message.Identifier);

        try
        {
            //发送转发封包
            await WriterAsync(message, tokenSource.Token);
        }
        catch (Exception e)
        {
            messageAwaitable.Fail(e);
            this.LogError(e,
                $"[{RemoteEndPoint}]: commandKey= {message.Key};Identifier= {message.Identifier} WaitAsync 发送封包抛出一个异常");
        }

        try
        {
            //等待封包结果
            return await messageAwaitable.WaitAsync(tokenSource.Token);
        }
        catch (Exception e)
        {
            if (e is TimeoutException)
                this.LogError(
                    $"[{RemoteEndPoint}]: commandKey= {message.Key};Identifier= {message.Identifier} WaitAsync Timeout");

            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal ValueTask<bool> DispatchAsync(CommandMessage message)
    {
        var result = _messageDispatcher.TryDispatch(message);

        return ValueTask.FromResult(result);
    }
    
    #endregion
    
    public async ValueTask DisposeAsync()
    {
        await _reader.DisposeAsync();
        await _writer.DisposeAsync();
        await connection.DisposeAsync();
    }

    #region ILogger

    ILogger GetLogger()
    {
        return logger;
    }

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        GetLogger().Log(logLevel, eventId, state, exception,
            (s, e) => $"Session[{connection.ConnectionId}]: {formatter(s, e)}");
    }

    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        return GetLogger().IsEnabled(logLevel);
    }

    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        return GetLogger().BeginScope(state);
    }

    public ILogger Logger => this;

    #endregion
}