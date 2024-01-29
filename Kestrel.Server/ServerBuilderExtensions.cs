using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using NamedPipeEndPoint = Bedrock.Framework.NamedPipeEndPoint;

namespace KestrelServer;

public static partial class ServerBuilderExtensions
{
    public static ServerBuilder ListenNamedPipe(this ServerBuilder builder, NamedPipeEndPoint namedPipeEndPoint, Action<IConnectionBuilder> serverApplication)
    {
        return builder.Listen<NamedPipeConnectionListenerFactory>(namedPipeEndPoint, serverApplication);
    }
}