using System.Diagnostics.CodeAnalysis;

namespace KestrelServer;

/// <summary>
/// 表示应用程序创建者
/// </summary>
public class ApplicationBuilder<TContext>
{
    private readonly ApplicationDelegate<TContext> _fallbackHandler;
    private readonly List<Func<ApplicationDelegate<TContext>, ApplicationDelegate<TContext>>> _middlewares = new();

    /// <summary>
    /// 获取服务提供者
    /// </summary>
    public IServiceProvider ApplicationServices { get; }

    /// <summary>
    /// 应用程序创建者
    /// </summary>
    /// <param name="appServices"></param>
    public ApplicationBuilder(IServiceProvider appServices)
        : this(appServices, context => ValueTask.CompletedTask)
    {
    }

    /// <summary>
    /// 应用程序创建者
    /// </summary>
    /// <param name="appServices"></param>
    /// <param name="fallbackHandler">回退处理者</param>
    public ApplicationBuilder(IServiceProvider appServices, ApplicationDelegate<TContext> fallbackHandler)
    {
        ApplicationServices = appServices;
        _fallbackHandler = fallbackHandler;
    }

    /// <summary>
    /// 创建处理应用请求的委托
    /// </summary>
    /// <returns></returns>
    public ApplicationDelegate<TContext> Build()
    {
        var handler = _fallbackHandler;
        for (var i = this._middlewares.Count - 1; i >= 0; i--)
        {
            handler = _middlewares[i](handler);
        }

        return handler;
    }


    /// <summary>
    /// 使用默认配制创建新的PipelineBuilder
    /// </summary>
    /// <returns></returns>
    public ApplicationBuilder<TContext> New()
    {
        return new ApplicationBuilder<TContext>(ApplicationServices, _fallbackHandler);
    }

    /// <summary>
    /// 条件中间件
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="handler"></param> 
    /// <returns></returns>
    public ApplicationBuilder<TContext> When(Func<TContext, bool> predicate, ApplicationDelegate<TContext> handler)
    {
        return Use(next => async context =>
        {
            if (predicate(context))
            {
                await handler(context);
            }
            else
            {
                await next(context);
            }
        });
    }


    /// <summary>
    /// 条件中间件
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="configureAction"></param>
    /// <returns></returns>
    public ApplicationBuilder<TContext> When(Func<TContext, bool> predicate,
        Action<ApplicationBuilder<TContext>> configureAction)
    {
        return Use(next => async context =>
        {
            if (predicate(context))
            {
                var branchBuilder = New();
                configureAction(branchBuilder);
                await branchBuilder.Build().Invoke(context);
            }
            else
            {
                await next(context);
            }
        });
    }

    /// <summary>
    /// 使用中间件
    /// </summary>
    /// <typeparam name="TMiddleware"></typeparam>
    /// <returns></returns>
    public ApplicationBuilder<TContext> Use<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMiddleware>()
        where TMiddleware : IApplicationMiddleware<TContext>
    {
        var middleware = ActivatorUtilities.GetServiceOrCreateInstance<TMiddleware>(ApplicationServices);
        return Use(middleware);
    }

    /// <summary>
    /// 使用中间件
    /// </summary> 
    /// <typeparam name="TMiddleware"></typeparam> 
    /// <param name="middleware"></param>
    /// <returns></returns>
    public ApplicationBuilder<TContext> Use<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TMiddleware>(
        TMiddleware middleware)
        where TMiddleware : IApplicationMiddleware<TContext>
    {
        return Use(middleware.InvokeAsync);
    }

    /// <summary>
    /// 使用中间件
    /// </summary>  
    /// <param name="middleware"></param>
    /// <returns></returns>
    public ApplicationBuilder<TContext> Use(Func<ApplicationDelegate<TContext>, TContext, ValueTask> middleware)
    {
        return Use(next => async context => await middleware(next, context));
    }

    /// <summary>
    /// 使用中间件
    /// </summary>
    /// <param name="middleware"></param>
    /// <returns></returns>
    public ApplicationBuilder<TContext> Use(
        Func<ApplicationDelegate<TContext>, ApplicationDelegate<TContext>> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }
}