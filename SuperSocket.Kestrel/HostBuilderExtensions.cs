using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket.Kestrel;

public static class HostBuilderExtensions
{
    public static ISuperSocketHostBuilder UseKestrelChannelCreatorFactory(this ISuperSocketHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, server) =>
        {
            server.AddOptions<SocketTransportOptions>();
            server.AddSingleton<IConnectionListenerFactory, SocketTransportFactory>();
        });

        return hostBuilder.UseChannelCreatorFactory<KestrelChannelCreatorFactory>();
    }
}