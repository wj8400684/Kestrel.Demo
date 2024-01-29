using System.Net;
using Bedrock.Framework.Protocols;
using Kestrel.Core;
using Kestrel.Core.Messages;
using KestrelCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Client;
using SuperSocket.IOCPEasyClient;

namespace Kestrel.Client;

public class MessageDispatchClient2
{
    private readonly IServiceProvider _provider;
    private readonly IConnectionFactory _connectionFactory;
    private readonly CommandEncoder _encoder = new();
    private readonly MessageDispatcher _messageDispatcher = new();
    private readonly MessageIdentifierProvider _messageIdentifierProvider = new();
    private readonly FixedHeaderPipelineFilter _pipelineFilter = new(new CommandMessageFactoryPool());

    private ProtocolReader _reader;
    private ProtocolWriter _writer;

    public MessageDispatchClient2()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddSocketConnectionFactory();
        service.ConfigureOptions<SocketTransportOptionsSetup>();
       
        _provider = service.BuildServiceProvider();
        _connectionFactory = _provider.GetRequiredService<IConnectionFactory>();
    }

    public async ValueTask<ValueStartResult> StartAsync()
    {
        ConnectionContext connectionContext;

        try
        {
            connectionContext = await _connectionFactory.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8081));
        }
        catch (Exception e)
        {
            return ValueStartResult.SetError(e);
        }

        _reader = connectionContext.CreateReader();
        _writer = connectionContext.CreateWriter();

        StartReceive(connectionContext);

        return ValueStartResult.SetResult(true);
    }

    private async void StartReceive(ConnectionContext connectionContext)
    {
        while (!connectionContext.ConnectionClosed.IsCancellationRequested)
        {
            try
            {
                var readResult = await _reader.ReadAsync(_pipelineFilter);

                if (readResult.IsCanceled)
                    break;

                if (readResult.Message is not CommandRespMessageWithIdentifier respMessageWithIdentifier)
                    continue;

                await TryDispatchAsync(respMessageWithIdentifier);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _reader.Advance();
            }
        }

        Console.WriteLine("断开连接");
    }

    private void OnClosed(object sender, EventArgs e)
    {
        Console.WriteLine("断开连接");
    }

    private async ValueTask OnPackageHandler(EasyClient<CommandMessage> sender, CommandMessage package)
    {
        if (package is not CommandRespMessageWithIdentifier respMessageWithIdentifier)
            return;

        await TryDispatchAsync(respMessageWithIdentifier);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private ValueTask TryDispatchAsync(CommandRespMessageWithIdentifier request)
    {
        _messageDispatcher.TryDispatch(request);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 获取响应封包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="request"></param>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="Exception"></exception>
    /// <returns></returns>
    public ValueTask<TReplyMessage> GetResponseAsync<TReplyMessage>(CommandMessageWithIdentifier request)
        where TReplyMessage : CommandRespMessageWithIdentifier
    {
        return GetResponseAsync<TReplyMessage>(request, CancellationToken.None);
    }

    /// <summary>
    /// 获取响应封包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="Exception"></exception>
    /// <returns></returns>
    public async ValueTask<TReplyMessage> GetResponseAsync<TReplyMessage>(
        CommandMessageWithIdentifier request,
        CancellationToken cancellationToken)
        where TReplyMessage : CommandRespMessageWithIdentifier
    {
        cancellationToken.ThrowIfCancellationRequested();

        request.Identifier = _messageIdentifierProvider.GetNextIdentifier();

        using var messageAwaitable = _messageDispatcher.AddAwaitable<TReplyMessage>(request.Identifier);

        try
        {
            await _writer.WriteAsync(_pipelineFilter, request, CancellationToken.None);
        }
        catch (Exception e)
        {
            messageAwaitable.Fail(e);
            throw new Exception("发送封包抛出一个异常", e);
        }

        try
        {
            return await messageAwaitable.WaitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            if (e is TimeoutException)
                throw new TimeoutException($"等待封包调度超时命令：{request.Key}", e);

            throw new Exception("等待封包调度抛出一个异常", e);
        }
    }
}