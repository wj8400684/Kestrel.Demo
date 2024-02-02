using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Quic;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Kestrel.NamedPipe;

namespace SuperSocket.Kestrel;

public static class HostBuilderExtensions
{
    public static ISuperSocketHostBuilder UseTcpChannelCreatorFactory(this ISuperSocketHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, server) =>
        {
            server.AddOptions<SocketTransportOptions>();
            server.AddSingleton<IConnectionListenerFactory, SocketTransportFactory>();
        });

        return hostBuilder.UseChannelCreatorFactory<KestrelSocketChannelCreatorFactory>();
    }
    
    public static ISuperSocketHostBuilder UseQuicChannelCreatorFactory(this ISuperSocketHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, server) =>
        {
            server.AddSocketConnectionFactory();
            server.AddOptions<QuicTransportOptions>();
        });

        return hostBuilder.UseChannelCreatorFactory<KestrelQuicChannelCreatorFactory>();
    }
    
    public static ISuperSocketHostBuilder UseNamedPipeChannelCreatorFactory(this ISuperSocketHostBuilder hostBuilder)
    {
        return hostBuilder.UseChannelCreatorFactory<NamedPipeChannelCreatorFactory>();
    }
}