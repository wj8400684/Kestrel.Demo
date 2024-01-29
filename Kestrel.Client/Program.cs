using System.Diagnostics;
using System.Net;
using Bedrock.Framework;
using Bedrock.Framework.Protocols;
using Kestrel.Client;
using KestrelCore;
using Microsoft.Extensions.Logging;
using SuperSocket.Client;
using SuperSocket.IOCPEasyClient;

var client = new MessageDispatchClient();

var result = await client.StartAsync();

if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
    Console.ReadKey();
}

Console.WriteLine("连接成功");

var sendCount = 0;
var watch = new Stopwatch();

Console.WriteLine("开始发送数据");

watch.Start();

while (watch.Elapsed.TotalSeconds < 60)
{
    sendCount++;

    var requestMessage = CommandMessage.NewMessage(CommandType.Login, new LoginMessageRequest
    {
        Username = "wujun",
        Password = "ssss",
    });

    var commandResponse = await client.GetResponseAsync<LoginMessageReply>(requestMessage);

    var content = commandResponse.DecodeMessage();

}

watch.Stop();

Console.WriteLine($"支持完毕总共发送{sendCount}");

Console.ReadKey();
//
//
// var encoder = new CommandEncoder();
//
// var client = new IOCPTcpEasyClient<CommandMessage, CommandMessage>(new CommandFilterPipeLine(), encoder).AsClient();
//
// await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8081));
//
// Console.WriteLine("连接成功");
//
// while (true)
// {
//     var requestMessage = CommandMessage.NewMessage(CommandType.Login, new LoginMessageRequest
//     {
//         Username = "wujun",
//         Password = "ssss",
//     });
//
//     await client.SendAsync(encoder, requestMessage);
//     var resp = await client.ReceiveAsync();
//     
//     await Task.Delay(1000);
// }

// var encoder = new CommandEncoder();
//
// var client = new IOCPTcpEasyClient<CommandMessage, CommandMessage>(new CommandFilterPipeLine(), encoder).AsClient();
//
// var connection = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8081));
//
// if (!connection)
// {
//     Console.WriteLine("连接失败");
//     Console.ReadKey();
// }
//
// Console.WriteLine("连接成功");
//
// while (true)
// {
//     var requestMessage = CommandMessage.NewMessage(CommandType.Login, new LoginMessageRequest
//     {
//         Username = "wujun",
//         Password = "ssss",
//     });
//
//     await client.SendAsync(encoder, requestMessage);
//     var resp = await client.ReceiveAsync();
//     
//     await Task.Delay(1000);
// }

//
// var sendCount = 0;
// var watch = new Stopwatch();
//
// Console.WriteLine("开始发送数据");
//
// watch.Start();
//
// while (watch.Elapsed.TotalSeconds < 60)
// {
//     sendCount++;
//     var requestMessage = CommandMessage.NewMessage(CommandType.Login, new LoginMessageRequest
//     {
//         Username = "wujun",
//         Password = "ssss",
//     });
//
//     await client.SendAsync(encoder, requestMessage);
//     var resp = await client.ReceiveAsync();
// }
//
// watch.Stop();
//
// Console.WriteLine($"支持完毕总共发送{sendCount}");
//
// Console.ReadKey();
//
//
//
// var loggerFactory = LoggerFactory.Create(builder =>
// {
//     builder.SetMinimumLevel(LogLevel.Debug);
//     builder.AddConsole();
// });
//
// var client = new ClientBuilder()
//     //.UseConnectionFactory(new NamedPipeConnectionFactory())
//     .UseSockets()
//     .Build();
//
// //await using var connection = await client.ConnectAsync(new NamedPipeEndPoint("ss"));
// await using var connection = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 8081));
// Console.WriteLine($"Connected to {connection.LocalEndPoint}");
//
// //var protocol = new LengthPrefixedProtocol(new DefaultPacketFactoryPool());
//
// var protocol = new FixedHeaderPipelineFilter();
//
// var reader = connection.CreateReader();
// var writer = connection.CreateWriter();
//
// var sendCount = 0;
// var watch = new Stopwatch();
//
// Console.WriteLine("开始发送数据");
//
// watch.Start();
//
// while (watch.Elapsed.TotalSeconds < 60)
// {
//     sendCount++;
//     var requestMessage = CommandMessage.NewMessage(CommandType.Login, new LoginMessageRequest
//     {
//         Username = "wujun",
//         Password = "ssss",
//     });
//
//     await writer.WriteAsync(protocol, requestMessage);
//
//     var result = await reader.ReadAsync(protocol);
//
//     if (result.IsCompleted)
//         break;
//
//     reader.Advance();
// }
//
// watch.Stop();
//
// Console.WriteLine($"支持完毕总共发送{sendCount}");
//
// Console.ReadKey();