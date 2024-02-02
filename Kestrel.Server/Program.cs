using System.Net;
using System.Net.Quic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Bedrock.Framework;
using Google.Protobuf;
using Kestrel.Core;
using KestrelCore;
using KestrelServer;
using KestrelServer.Commands;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Quic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.IOCPTcpChannelCreatorFactory;
using SuperSocket.Kestrel;
using SuperSocket.Udp;

var host = SuperSocketHostBuilder.Create<CommandMessage, CommandFilterPipeLine>()
    .UseCommand(options => options.AddCommand<KestrelServer.SSServer.LoginCommand>())
    .UsePackageEncoder<CommandEncoder>()
    .UseSessionFactory<KestrelServer.SSServer.TestSessionFactory>()
    .UseQuicChannelCreatorFactory()
    .Build();

await host.RunAsync();

//
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Debug);
    builder.AddConsole();
});

services.TryAddEnumerable(ServiceDescriptor.Singleton<IAsyncCommand, LoginCommand>());

var serviceProvider = services.BuildServiceProvider();
//
// var endpoint = new IPEndPoint(IPAddress.Any, 12345); // 监听IP地址和端口号
//  var quicOptions = new QuicTransportOptions(); // 创建QUIC传输选项
// //
// //
//\  new QuicTransportFactory()
//
// var quicListener = new QuicTransportFactory(quicOptions).Create(endpoint); // 创建QUIC监听器
// //
// var kestrelOptions = new KestrelServerOptions(); // 创建Kestrel选项
// kestrelOptions.Listen(quicListener,
//     builder => builder.UseConnectionHandler<QuicServerHandler>()); // 将QUIC监听器添加到Kestrel中
//
// var server = new KestrelServer(kestrelOptions); // 创建Kestrel服务器
//
// await server.StartAsync(new QuicServerHandler()); // 启动服务器
//
// // var server = new ServerBuilder(serviceProvider)
// //     .ListenNamedPipe(new Bedrock.Framework.NamedPipeEndPoint("ss"),
// //         s => { s.UseConnectionHandler<CommandConnectionHandler>(); }
// //     )
// //     .Build();
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

builder.Services.AddLogging();
builder.Services.AddSingleton<KestrelServer.ISessionContainer, InProcSessionContainer>();
builder.Services.AddCommands<LoginCommand>();
builder.Services.AddKestrelCommands<KestrelLoginCommand>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081, l => l.UseConnectionHandler<KestrelChannelConnectionHandler>());
});

var app = builder.Build();

app.Run();