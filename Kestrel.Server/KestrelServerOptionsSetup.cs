using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace KestrelServer;

public sealed class KestrelServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
{
    public void Configure(KestrelServerOptions options)
    {
        options.ListenAnyIP(8081, listen =>
        {
            listen.UseConnectionHandler<CommandConnectionHandler>();
        });
    }
}