using KestrelCore;

namespace KestrelServer.Middlewares;

public sealed class AuthorMiddleware(ILogger<AuthorMiddleware> logger) : IApplicationMiddleware<CommandContext>
{
    async ValueTask IApplicationMiddleware<CommandContext>.InvokeAsync(ApplicationDelegate<CommandContext> next,
        CommandContext context)
    {
        //客户端还未登录
        if (!context.Channel.IsLogin)
        {
            //当前命令不是登陆命令
            if (context.Message.Key != CommandType.Login)
            {
                logger.LogWarning("需要登陆...");
                return;
            }
        }
        else if (context.Message.Key == CommandType.Login)//已经登陆但是还需要登陆
        {
            logger.LogWarning("已经登陆...");
            return;
        }
        
        //只要登陆命令才能过
        await next(context);
    }
}