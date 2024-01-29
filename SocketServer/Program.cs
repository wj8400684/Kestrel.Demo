using Kestrel.Core;
using Kestrel.Core.Messages;
using KestrelCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocketServer;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.IOCPTcpChannelCreatorFactory;

var host = SuperSocketHostBuilder.Create<CommandMessage, CommandFilterPipeLine>()
    .UsePackageEncoder<CommandEncoder>()
    .UsePackageDecoder<CommandDecoder>()
    .UseCommand(options => options.AddCommand<LoginCommand>())
    .UseSessionFactory<TestSessionFactory>()
    .UseIOCPTcpChannelCreatorFactory()
    .UseInProcSessionContainer()
    .ConfigureServices((_, service) =>
    {
        service.AddSingleton<IMessageFactoryPool, CommandMessageFactoryPool>();
    })
    .Build();

await host.RunAsync();