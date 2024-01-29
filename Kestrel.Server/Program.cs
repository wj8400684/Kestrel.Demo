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
// var host = SuperSocketHostBuilder.Create<CommandMessage, CommandFilterPipeLine>()
//     .UsePackageEncoder<CommandEncoder>()
//     .UsePackageDecoder<CommandDecoder>()
//     .UseCommand(options => options.AddCommand<KestrelServer.SSServer.LoginCommand>())
//     .UseSessionFactory<KestrelServer.SSServer.TestSessionFactory>()
//     .UseIOCPTcpChannelCreatorFactory()
//     .UseInProcSessionContainer()
//     .ConfigureServices((_, service) =>
//     {
//         service.ConfigureOptions<ServerOptionsSetup>();
//         service.AddSingleton<IMessageFactoryPool, CommandMessageFactoryPool>();
//     })
//     .Build();
//
// await host.RunAsync();
//

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Debug);
    builder.AddConsole();
});

services.AddLogging();
services.AddSingleton<IMessageFactoryPool, CommandMessageFactoryPool>();
services.AddSingleton<KestrelServer.ISessionContainer, InProcSessionContainer>();
services.AddCommands<LoginCommand>();

var serviceProvider = services.BuildServiceProvider();

var socketPath = Path.Combine(Path.GetTempPath(), "socket.tmp");
//

if (File.Exists(socketPath))
    File.Delete(socketPath);

var server = new ServerBuilder(serviceProvider)
    .UseSockets(l => { l.ListenUnixSocket(socketPath, c => c.UseConnectionHandler<CommandConnectionHandler>()); })
    .Build();

await server.StartAsync();

var tcs = new TaskCompletionSource();
Console.WriteLine($"启动成功-{socketPath}");
Console.CancelKeyPress += (sender, e) => tcs.TrySetResult();
await tcs.Task;
//
// var builder = WebApplication.CreateSlimBuilder(args);
//
// // builder.Services.AddKestrelSocketServer<CommandMessage, FixedHeaderPipelineFilter>()
// //                 .UseSession<AppSession>()
// //                 .UseClearIdleSession()
// //                 .UseInProcSessionContainer()
// //                 .ConfigServer(service =>
// //                 {
// //                     service.AddLogging();
// //                     service.AddSession();
// //                 });
//
// builder.Services.AddLogging();
// builder.Services.ConfigureOptions<KestrelServerOptionsSetup>();
// builder.Services.AddSingleton<IMessageFactoryPool, CommandMessageFactoryPool>();
// builder.Services.AddSingleton<KestrelServer.ISessionContainer, InProcSessionContainer>();
// builder.Services.AddCommands<LoginCommand>();
//
// var app = builder.Build();
//
// app.Run();