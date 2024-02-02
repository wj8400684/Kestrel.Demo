using System.IO.Pipelines;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using Bedrock.Framework.Protocols;
using Google.Protobuf;
using Kestrel.Core;
using KestrelCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;
using SuperSocket.Channel.Kestrel;
using SuperSocket.Client;
using SuperSocket.IOCPEasyClient;
using SuperSocket.ProtoBase;

namespace Kestrel.Client;

public class MessageDispatchClient2
{
    private IChannel<CommandMessage> _channel;

    private readonly IServiceProvider _provider;
    private readonly IConnectionFactory _connectionFactory;
    private readonly MessageDispatcher _messageDispatcher = new();
    private readonly MessageIdentifierProvider _messageIdentifierProvider = new();
    private readonly IPackageEncoder<CommandMessage> _encoder = new CommandEncoder();

    public MessageDispatchClient2()
    {
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddSocketConnectionFactory();

        _provider = service.BuildServiceProvider();
        _connectionFactory = _provider.GetRequiredService<IConnectionFactory>();
    }

    public async ValueTask<ValueStartResult> StartAsync()
    {
// This represents the minimal configuration necessary to open a connection.
#pragma warning disable CA2252
        var clientConnectionOptions = new QuicClientConnectionOptions
        {
            // End point of the server to connect to.
            RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 8081),

            DefaultCloseErrorCode = 0,
            DefaultStreamErrorCode = 0,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions
            {
                ApplicationProtocols = new List<SslApplicationProtocol> { SslApplicationProtocol.Http3 },
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => { return true; }
            }
        };

        var connection = await QuicConnection.ConnectAsync(clientConnectionOptions);

        var outgoingStream = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional);

        //var inboundStream = await connection.AcceptInboundStreamAsync();

        var connectionContext = new QuicStreamConnectionContext(outgoingStream, outgoingStream,
            connection.LocalEndPoint,
            connection.RemoteEndPoint);

        _channel =
            new KestrelPipeChannel<CommandMessage>(connectionContext, new CommandFilterPipeLine(),
                new ChannelOptions());

        _channel.Closed += OnClosed;

        StartReceive();

        return ValueStartResult.SetResult(true);

#pragma warning restore CA2252

// Initialize, configure and connect to the server.

        //
        //
        //     ConnectionContext connectionContext;
        //
        //     try
        //     {
        //         connectionContext = await _connectionFactory.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8081));
        //     }
        //     catch (Exception e)
        //     {
        //         return ValueStartResult.SetError(e);
        //     }
        //
        //     _channel =
        //         new KestrelPipeChannel<CommandMessage>(connectionContext, new CommandFilterPipeLine(),
        //             new ChannelOptions());
        //
        //     _channel.Closed += OnClosed;
        //
        //     StartReceive();
        //
        //     return ValueStartResult.SetResult(true);
    }

    private async void StartReceive()
    {
        _channel.Start();

        try
        {
            await foreach (var message in _channel.RunAsync())
            {
                await TryDispatchAsync(message);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        Console.WriteLine("断开连接");
    }

    private void OnClosed(object sender, EventArgs e)
    {
        Console.WriteLine("断开连接");
    }

    private async ValueTask OnPackageHandler(EasyClient<CommandMessage> sender, CommandMessage package)
    {
        await TryDispatchAsync(package);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private ValueTask TryDispatchAsync(CommandMessage message)
    {
        _messageDispatcher.TryDispatch(message);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 获取响应封包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="message"></param>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="Exception"></exception>
    /// <returns></returns>
    public ValueTask<ValueCommandResponse<TReplyMessage>> GetResponseAsync<TReplyMessage>(CommandMessage message)
        where TReplyMessage : IMessage<TReplyMessage>
    {
        return GetResponseAsync<TReplyMessage>(message, CancellationToken.None);
    }

    /// <summary>
    /// 获取响应封包
    /// </summary>
    /// <typeparam name="TReplyMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="Exception"></exception>
    /// <returns></returns>
    public async ValueTask<ValueCommandResponse<TReplyMessage>> GetResponseAsync<TReplyMessage>(
        CommandMessage message,
        CancellationToken cancellationToken)
        where TReplyMessage : IMessage<TReplyMessage>
    {
        cancellationToken.ThrowIfCancellationRequested();

        message.Identifier = _messageIdentifierProvider.GetNextIdentifier();

        using var messageAwaitable = _messageDispatcher.AddAwaitable<TReplyMessage>(message.Identifier);

        try
        {
            await _channel.SendAsync(_encoder, message);
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
                throw new TimeoutException($"等待封包调度超时命令：{message.Key}", e);

            throw new Exception("等待封包调度抛出一个异常", e);
        }
    }
}