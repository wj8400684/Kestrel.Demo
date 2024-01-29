using System.Net;
using System.Security.Cryptography.X509Certificates;
using Bedrock.Framework;
using Google.Protobuf;
using KestrelCore;
using KestrelServer;
using KestrelServer.Commands;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.IOCPTcpChannelCreatorFactory;

// var host = SuperSocketHostBuilder.Create<CommandMessage, CommandFilterPipeLine>()
//     .UseCommand(options => options.AddCommand<KestrelServer.SSServer.LoginCommand>())
//     .UsePackageEncoder<CommandEncoder>()
//     .UseSessionFactory<KestrelServer.SSServer.TestSessionFactory>()
//     .UseIOCPTcpChannelCreatorFactory()
//     .Build();
//
// await host.RunAsync();
//
//
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Debug);
    builder.AddConsole();
});

services.TryAddEnumerable(ServiceDescriptor.Singleton<IAsyncCommand, LoginCommand>());

var serviceProvider = services.BuildServiceProvider();

var server = new ServerBuilder(serviceProvider)
    .ListenNamedPipe(new Bedrock.Framework.NamedPipeEndPoint("ss"),
        s => { s.UseConnectionHandler<CommandConnectionHandler>(); }
    )
    .Build();

var server = new ServerBuilder(serviceProvider)
    .UseSockets(l => { l.ListenAnyIP(8081, c => c.UseConnectionHandler<CommandConnectionHandler>()); })
    .Build();

await server.StartAsync();
//
// var tcs = new TaskCompletionSource();
// Console.WriteLine("启动成功");
// Console.CancelKeyPress += (sender, e) => tcs.TrySetResult();
// await tcs.Task;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddCommands<LoginCommand>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081, l => l.UseConnectionHandler<CommandConnectionHandler>());
});

var app = builder.Build();

app.Run();