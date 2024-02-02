namespace KestrelServer;

public delegate ValueTask ApplicationDelegate<in TContext>(TContext context);