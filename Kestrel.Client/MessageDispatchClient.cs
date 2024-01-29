using System.Net;
using Kestrel.Core;
using Kestrel.Core.Messages;
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
        _client = new EasyClient<CommandMessage, CommandMessage>(new CommandFilterPipeLine
        {
            Decoder = new CommandDecoder(new CommandMessageFactoryPool())
        }, _encoder);
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
            await _client.SendAsync(request);
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