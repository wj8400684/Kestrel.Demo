using Microsoft.Extensions.Options;
using SuperSocket;

namespace KestrelServer;

public sealed class ServerOptionsSetup : IConfigureOptions<ServerOptions>
{
    public void Configure(ServerOptions options)
    {
        options.Listeners =
        [
            new ListenOptions
            {
                Ip = "Any",
                Port = 8081,
            }
        ];
    }
}