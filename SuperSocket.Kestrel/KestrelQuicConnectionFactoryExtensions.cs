using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Quic;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket.Kestrel;

internal static class KestrelQuicConnectionFactoryExtensions
{
    private const string QuicConnectionFactoryTypeName = "Microsoft.AspNetCore.Server.Kestrel.Transport.Quic.QuicTransportFactory";

    /// <summary>
    /// 查找SocketConnectionFactory的类型
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static Type FindQuicConnectionFactory()
    {
        var assembly = typeof(QuicTransportOptions).Assembly;
        var connectionFactoryType = assembly.GetType(QuicConnectionFactoryTypeName);
        return connectionFactoryType ?? throw new NotSupportedException($"找不到类型{QuicConnectionFactoryTypeName}");
    }
    
    /// <summary>
    /// 注册SocketConnectionFactory为IConnectionFactory
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    // [DynamicDependency(DynamicallyAccessedMemberTypes.All,
    //     typeName: "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory",
    //     assemblyName: "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets")]
    public static IServiceCollection AddSocketConnectionFactory(this IServiceCollection services)
    {
        var factoryType = FindQuicConnectionFactory();

        return services.AddSingleton(typeof(IMultiplexedConnectionListenerFactory), factoryType);
    }
}