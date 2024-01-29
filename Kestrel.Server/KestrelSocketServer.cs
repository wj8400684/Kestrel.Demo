using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace KestrelServer;
//
// public static class KestrelSocketServerBuildExtensions
// {
//     public static KestrelSocketServerBuild<TReceiveMessage> AddKestrelSocketServer<TReceiveMessage>(
//         this IServiceCollection serviceCollection)
//         where TReceiveMessage : class
//     {
//         return KestrelSocketServerBuild<TReceiveMessage>.Create(serviceCollection);
//     }
//
//     public static KestrelSocketServerBuild<TReceiveMessage> AddKestrelSocketServer<TReceiveMessage, TMessageProtocol>(
//         this IServiceCollection serviceCollection)
//         where TReceiveMessage : class
//     {
//         return KestrelSocketServerBuild<TReceiveMessage>.Create(serviceCollection).UseMessageProtocol<TMessageProtocol>();
//     }
// }
//
// public interface IAppSession
// {
// }
//
// public struct AppSession : IAppSession
// {
// }
//
// public class KestrelSocketServerBuild<TReceiveMessage>(IServiceCollection serviceCollection)
// {
//     public static KestrelSocketServerBuild<TReceiveMessage> Create(IServiceCollection? serviceCollection = null)
//     {
//         serviceCollection ??= new ServiceCollection();
//
//         serviceCollection.AddSingleton<KestrelSocketServer<TReceiveMessage>>();
//
//         return new KestrelSocketServerBuild<TReceiveMessage>(serviceCollection);
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> ConfigKestrelServerOptions(Action<KestrelServerOptions> config)
//     {
//         serviceCollection.AddOptions<KestrelServerOptions>();
//         serviceCollection.Configure(config);
//
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> ConfigServer(Action<IServiceCollection> config)
//     {
//         config.Invoke(serviceCollection);
//
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseSession<TSession>()
//         where TSession : IAppSession
//     {
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseMessageReader<TMessageReader>()
//         where TMessageReader : IMessageReader<TReceiveMessage>
//     {
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseMessageWriter<TMessageWriter>()
//         where TMessageWriter : IMessageWriter<TReceiveMessage>
//     {
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseMessageProtocol<TMessageProtocol>()
//     {
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseInProcSessionContainer()
//     {
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseCommands()
//     {
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseChannelCreatorFactory<TChannelCreatorFactory>()
//         where TChannelCreatorFactory : IKestrelChannelCreatorFactory
//     {
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseClearIdleSession()
//     {
//         return this;
//     }
//
//     public KestrelSocketServerBuild<TReceiveMessage> UseMiddleware<TMiddleware>()
//     {
//         return this;
//     }
// }
//
// public interface IKestrelAppChannel
// {
// }
//
// public interface IKestrelChannelCreatorFactory
// {
//     event Func<IKestrelAppChannel, ValueTask> NewClientAccepted;
// }
//
// public class KestrelChannelCreatorFactory : ConnectionHandler, IKestrelChannelCreatorFactory
// {
//     public KestrelChannelCreatorFactory()
//     {
//     }
//
//     public override Task OnConnectedAsync(ConnectionContext connection)
//     {
//         return Task.CompletedTask;
//     }
//
//     public event Func<IKestrelAppChannel, ValueTask>? NewClientAccepted;
// }
//
// public class KestrelSocketServer<TReceiveMessage>(IServiceProvider serviceProvider)
// {
//     
// }