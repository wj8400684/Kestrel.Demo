using KestrelServer;
using KestrelServer.Commands;
using KestrelServer.Options;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddLogging();
builder.Services.AddKestrelCommands<KestrelLoginCommand>();
builder.Services.ConfigureOptions<KestrelServerOptionsSetup>();

var app = builder.Build();

app.Run();