using System.Net;
using Google.Protobuf;
using Kestrel.Core;
using KestrelCore;
using SuperSocket.Client;
using SuperSocket.IOCPEasyClient;

namespace Kestrel.Client;

public class MessageDispatchClient
{
    private readonly CommandEncoder _encoder = new();
    private readonly IEasyClient<CommandMessage, CommandMessage> _client;
    private readonly MessageDispatcher _messageDispatcher = new();
    private readonly MessageIdentifierProvider _messageIdentifierProvider = new();

    public MessageDispatchClient()
    {
        _client = new IOCPTcpEasyClient<CommandMessage, CommandMessage>(new CommandFilterPipeLine(), _encoder);
        _client.Closed += OnClosed;
        _client.PackageHandler += OnPackageHandler;
    }

    public async ValueTask<ValueStartResult> StartAsync()
    {
        bool connected;

        try
        {
            connected = await _client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8081));
        }
        catch (Exception e)
        {
            return ValueStartResult.SetError(e);
        }

        if (!connected)
            return ValueStartResult.SetResult(false);

        _client.StartReceive();

        return ValueStartResult.SetResult(true);
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
            await _client.SendAsync(message);
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