using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Options;

namespace Kestrel.Client;

public sealed class SocketTransportOptionsSetup : IConfigureOptions<SocketTransportOptions>
{
    public void Configure(SocketTransportOptions options)
    {
        
    }
}