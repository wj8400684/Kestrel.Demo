using System.Diagnostics.CodeAnalysis;
using System.Net.Quic;
using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace Kestrel.Core;

public static class SocketConnectionFactoryExtensions
{
    private const string SocketConnectionFactoryTypeName =
        "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory";

    /// <summary>
    /// 查找SocketConnectionFactory的类型
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static Type FindSocketConnectionFactory()
    {
        var assembly = typeof(SocketTransportOptions).Assembly;
        var connectionFactoryType = assembly.GetType(SocketConnectionFactoryTypeName);
        return connectionFactoryType ?? throw new NotSupportedException($"找不到类型{SocketConnectionFactoryTypeName}");
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

    public static IServiceCollection AddNamedPipeConnectionFactory(this IServiceCollection services)
    {
        return services.AddTransient<IConnectionFactory, NamedPipeConnectionFactory>();
    }
}