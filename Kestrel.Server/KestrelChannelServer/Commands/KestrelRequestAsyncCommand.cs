using Google.Protobuf;
using KestrelCore;
using SuperSocket.Channel;

namespace KestrelServer.Commands;

public abstract class KestrelRequestAsyncCommand : IKestrelAsyncCommand
{
    public abstract CommandType CommandType { get; }

    ValueTask IKestrelAsyncCommand.
        ExecuteAsync(IChannel session, CommandMessage package) =>
        SchedulerAsync(session, package, CancellationToken.None);

    /// <summary>
    /// 执行包命令
    /// </summary>
    /// <param name="session">连接回话</param>
    /// <param name="package">完整包</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask OnHandlerAsync(IChannel session, CommandMessage package,
        CancellationToken cancellationToken);

    /// <summary>
    /// 执行调度
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    protected virtual async ValueTask SchedulerAsync(IChannel session, CommandMessage package,
        CancellationToken cancellationToken)
    {
        await TryProcessPackageAsync(session, package, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask OnHandlerErrorAsync(
        IChannel session,
        CommandMessage package,
        Exception e,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 执行包命令
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    private async ValueTask TryProcessPackageAsync(
        IChannel session,
        CommandMessage package,
        CancellationToken cancellationToken)
    {
        try
        {
            await OnHandlerAsync(session, package, cancellationToken);
        }
        catch (Exception e)
        {
            await OnHandlerErrorAsync(session, package, e, cancellationToken);
        }
    }
}

public abstract class KestrelRequestAsyncCommand<TRequestPackage> : IKestrelAsyncCommand
    where TRequestPackage : IMessage<TRequestPackage>
{
    public abstract CommandType CommandType { get; }
    
    private readonly MessageParser<TRequestPackage> _requestParser =
        new(() => Activator.CreateInstance<TRequestPackage>()!);

    ValueTask IKestrelAsyncCommand.
        ExecuteAsync(IChannel session, CommandMessage package) =>
        SchedulerAsync(session, package, CancellationToken.None);

    /// <summary>
    /// 执行包命令
    /// </summary>
    /// <param name="session">连接回话</param>
    /// <param name="package">完整包</param>
    /// <param name="request">具体请求内容</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask OnHandlerAsync(IChannel session, CommandMessage package,
        TRequestPackage request, CancellationToken cancellationToken);

    /// <summary>
    /// 执行调度
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    protected virtual async ValueTask SchedulerAsync(IChannel session, CommandMessage package,
        CancellationToken cancellationToken)
    {
        await TryProcessPackageAsync(session, package, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="request"></param>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask OnHandlerErrorAsync(
        IChannel session,
        CommandMessage package,
        TRequestPackage request,
        Exception e,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 当解包发生错误时候触发
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask OnDecoderErrorAsync(
        IChannel session,
        CommandMessage package,
        Exception e,
        CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 解码包
    /// </summary>
    /// <param name="session"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask<TRequestPackage> OnDecoderPackageAsync(
        IChannel session,
        ByteString value,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_requestParser.ParseFrom(value));
    }

    /// <summary>
    /// 进行解包
    /// 执行包命令
    /// 编码响应包
    /// 发送响应包
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    private async ValueTask TryProcessPackageAsync(
        IChannel session,
        CommandMessage package,
        CancellationToken cancellationToken)
    {
        var request = await TryDecoderPackageAsync(session, package, cancellationToken);

        if (request == null)
            return;

        try
        {
            await OnHandlerAsync(session, package, request, cancellationToken);
        }
        catch (Exception e)
        {
            await OnHandlerErrorAsync(session, package, request, e, cancellationToken);
        }
    }

    /// <summary>
    /// 解析包如果解析失败则按照返回状态是否选择断开连接
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async ValueTask<TRequestPackage?> TryDecoderPackageAsync(
        IChannel session,
        CommandMessage package,
        CancellationToken cancellationToken)
    {
        try
        {
            return await OnDecoderPackageAsync(session, package.Content, cancellationToken);
        }
        catch (Exception e)
        {
            await OnDecoderErrorAsync(session, package, e, cancellationToken);
        }

        return default;
    }
}