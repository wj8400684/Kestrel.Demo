namespace KestrelServer;

/// <summary>
/// 表示可以处理应用请求的委托
/// </summary>
/// <typeparam name="TContext">中间件上下文类型</typeparam>
/// <param name="context">中间件上下文</param>
/// <returns></returns>
public delegate ValueTask ApplicationDelegate<TContext>(TContext context);