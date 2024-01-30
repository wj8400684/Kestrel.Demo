using KestrelServer;
using KestrelServer.Commands;
using Microsoft.AspNetCore.Connections;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddLogging();
builder.Services.AddConnections();
builder.Services.AddSingleton<ISessionContainer, InProcSessionContainer>();
builder.Services.AddCommands<LoginCommand>();

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(8081, l => l.UseConnectionHandler<CommandConnectionHandler>());
// });

var app = builder.Build();

app.UseRouting();

app.MapConnectionHandler<CommandConnectionHandler>("/api/websocket");

app.Run();