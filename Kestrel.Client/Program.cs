using System.Diagnostics;
using Kestrel.Client;
using Kestrel.Core.Messages;

var client = new MessageDispatchClient2();

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

    var commandResponse = await client.GetResponseAsync<LoginReplyMessage>(new LoginRequestMessage
    {
        Username = "wujun",
        Password = "wuun57889"
    });

    //var content = commandResponse.DecodeMessage();

}

watch.Stop();

Console.WriteLine($"支持完毕总共发送{sendCount}");

Console.ReadKey();