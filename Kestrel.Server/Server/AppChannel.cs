using Bedrock.Framework.Protocols;
using Kestrel.Core;
using Kestrel.Core.Messages;
using KestrelCore;
using Microsoft.AspNetCore.Connections;
using SuperSocket;

namespace KestrelServer.Server;

public sealed class AppChannel(
    ConnectionContext connection, 
    IMessageFactoryPool messageFactoryPool, 
    ILogger logger)
    : IAsyncDisposable, ILogger, ILoggerAccessor
{
    private readonly MessageDispatcher _messageDispatcher = new();
    private readonly MessageIdentifierProvider _messageIdentifierProvider = new();
    private readonly FixedHeaderPipelineFilter _pipelineFilter = new(messageFactoryPool);
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
    /// <param name="request"></param>
    /// <param name="responseTimeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal ValueTask<TReplyMessage> GetResponsePacketAsync<TReplyMessage>(
        CommandMessageWithIdentifier request,
        TimeSpan responseTimeout,
        CancellationToken cancellationToken)
        where TReplyMessage : CommandRespMessageWithIdentifier
    {
        using var timeOut = new CancellationTokenSource(responseTimeout);
        return GetResponsePacketAsync<TReplyMessage>(request, connection.ConnectionClosed, cancellationToken,
            timeOut.Token);
    }

    /// <summary>
    /// 获取响应包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal ValueTask<TReplyMessage> GetResponsePacketAsync<TReplyMessage>(
        CommandMessageWithIdentifier request,
        CancellationToken cancellationToken)
        where TReplyMessage : CommandRespMessageWithIdentifier
    {
        return GetResponsePacketAsync<TReplyMessage>(request, connection.ConnectionClosed, cancellationToken);
    }

    /// <summary>
    /// 获取响应包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="request"></param>
    /// <param name="tokens"></param>
    /// <returns></returns>
    internal async ValueTask<TReplyMessage> GetResponsePacketAsync<TReplyMessage>(
        CommandMessageWithIdentifier request,
        params CancellationToken[] tokens) where TReplyMessage : CommandRespMessageWithIdentifier
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokens);

        request.Identifier = _messageIdentifierProvider.GetNextIdentifier();

        using var messageAwaitable = _messageDispatcher.AddAwaitable<TReplyMessage>(request.Identifier);

        try
        {
            //发送转发封包
            await WriterAsync(request, tokenSource.Token);
        }
        catch (Exception e)
        {
            messageAwaitable.Fail(e);
            this.LogError(e,
                $"[{RemoteEndPoint}]: commandKey= {request.Key};Identifier= {request.Identifier} WaitAsync 发送封包抛出一个异常");
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
                    $"[{RemoteEndPoint}]: commandKey= {request.Key};Identifier= {request.Identifier} WaitAsync Timeout");

            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    internal ValueTask<bool> DispatchAsync(CommandRespMessageWithIdentifier response)
    {
        var result = _messageDispatcher.TryDispatch(response);

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