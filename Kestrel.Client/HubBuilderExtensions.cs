using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace Kestrel.Client;

public static class HubBuilderExtensions
{
    private const string socketConnectionFactoryTypeName = "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory";

    /// <summary>
    /// 查找SocketConnectionFactory的类型
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static Type FindSocketConnectionFactory()
    {
        var assembly = typeof(SocketTransportOptions).Assembly;
        var connectionFactoryType = assembly.GetType(socketConnectionFactoryTypeName);
        return connectionFactoryType ?? throw new NotSupportedException($"找不到类型{socketConnectionFactoryTypeName}");
    }
    
    /// <summary>
    /// 注册SocketConnectionFactory为IConnectionFactory
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All,
        typeName: "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory",
        assemblyName: "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets")]
    public static IServiceCollection AddSocketConnectionFactory(this IServiceCollection services)
    {
        var factoryType = FindSocketConnectionFactory();

        return services.AddTransient(typeof(IConnectionFactory), factoryType);
    }
}