using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace KestrelServer.Options;

internal sealed class KestrelServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
{
    public static readonly string SocketPath = Path.Combine(Path.GetTempPath(), "socket.tmp");
    
    public void Configure(KestrelServerOptions options)
    {
        options.ListenAnyIP(8081, listen =>
        {
            listen.UseConnectionHandler<ChannelConnectionHandler>();
        });
        
        // if (File.Exists(SocketPath))
        //     File.Delete(SocketPath);
        //
        // options.ListenUnixSocket(SocketPath, listen =>
        // {
        //     listen.UseConnectionHandler<KestrelChannelConnectionHandler>();
        // });
    }
}