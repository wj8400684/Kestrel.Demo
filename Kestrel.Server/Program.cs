using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Bedrock.Framework;
using Kestrel.Core;
using Kestrel.Core.Messages;
using KestrelCore;
using KestrelServer;
using KestrelServer.Commands;
using KestrelServer.Server;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.IOCPTcpChannelCreatorFactory;

//
var host = SuperSocketHostBuilder.Create<CommandMessage, CommandFilterPipeLine>()
    .UsePackageEncoder<CommandEncoder>()
    .UsePackageDecoder<CommandDecoder>()
    .UseCommand(options => options.AddCommand<KestrelServer.SSServer.LoginCommand>())
    .UseSessionFactory<KestrelServer.SSServer.TestSessionFactory>()
    .UseIOCPTcpChannelCreatorFactory()
    .UseInProcSessionContainer()
    .ConfigureServices((_, service) =>
    {
        service.ConfigureOptions<ServerOptionsSetup>();
        service.AddSingleton<IMessageFactoryPool, CommandMessageFactoryPool>();
    })
    .Build();

await host.RunAsync();
//
//
// var services = new ServiceCollection();
// services.AddLogging(builder =>
// {
//     builder.SetMinimumLevel(LogLevel.Debug);
//     builder.AddConsole();
// });
//
// services.TryAddEnumerable(ServiceDescriptor.Singleton<IAsyncCommand, LoginCommand>());
//
// var serviceProvider = services.BuildServiceProvider();

// var server = new ServerBuilder(serviceProvider)
//     .ListenNamedPipe(new Bedrock.Framework.NamedPipeEndPoint("ss"),
//         s => { s.UseConnectionHandler<CommandConnectionHandler>(); }
//     )
//     .Build();
//
// var server = new ServerBuilder(serviceProvider)
//     .UseSockets(l => { l.ListenAnyIP(8081, c => c.UseConnectionHandler<CommandConnectionHandler>()); })
//     .Build();
//
// await server.StartAsync();
//
// var tcs = new TaskCompletionSource();
// Console.WriteLine("启动成功");
// Console.CancelKeyPress += (sender, e) => tcs.TrySetResult();
// await tcs.Task;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddKestrelSocketServer<CommandMessage, FixedHeaderProtocol>()
                .UseSession<AppSession>()
                .UseClearIdleSession()
                .UseInProcSessionContainer()
                .ConfigServer(service =>
                {
                    service.AddLogging();
                    service.AddSession();
                });

builder.Services.AddLogging();
builder.Services.ConfigureOptions<KestrelServerOptionsSetup>();
builder.Services.AddSingleton<IMessageFactoryPool, CommandMessageFactoryPool>();
builder.Services.AddSingleton<KestrelServer.ISessionContainer, InProcSessionContainer>();
builder.Services.AddCommands<LoginCommand>();

var app = builder.Build();

app.Run();