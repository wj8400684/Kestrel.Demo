using Bedrock.Framework.Protocols;
using KestrelCore;
using Microsoft.AspNetCore.Connections;
using SuperSocket;

namespace KestrelServer.Server;

public sealed class AppChannel(ConnectionContext connection, ILogger logger)
    : IAsyncDisposable, ILogger, ILoggerAccessor
{
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