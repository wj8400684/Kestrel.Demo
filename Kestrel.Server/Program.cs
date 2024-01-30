using Kestrel.Core.Messages;
using Kestrel.Server.Server;
using KestrelServer;
using KestrelServer.Commands;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddLogging();
builder.Services.ConfigureOptions<KestrelServerOptionsSetup>();
builder.Services.AddSingleton<IMessageFactoryPool, CommandMessageFactoryPool>();
builder.Services.AddSingleton<ISessionContainer, InProcSessionContainer>();
builder.Services.AddCommands<LoginCommand>();

var app = builder.Build();

app.Run();