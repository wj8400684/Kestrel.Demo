using KestrelCore;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.Kestrel;

var host = SuperSocketHostBuilder.Create<CommandMessage, CommandFilterPipeLine>()
    .UseCommand(options => options.AddCommand<KestrelServer.SSServer.LoginCommand>())
    .UsePackageEncoder<CommandEncoder>()
    .UseSessionFactory<KestrelServer.SSServer.TestSessionFactory>()
    .UseSocketChannelCreatorFactory()
    .Build();

await host.RunAsync();
