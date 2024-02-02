using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.Channel.Kestrel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Kestrel;

internal sealed class KestrelQuicChannelCreatorFactory(IMultiplexedConnectionListenerFactory listenerFactory)
    : IChannelCreatorFactory
{
    IChannelCreator IChannelCreatorFactory.CreateChannelCreator<TPackageInfo>(
        ListenOptions options,
        ChannelOptions channelOptions, 
        ILoggerFactory loggerFactory, 
        object pipelineFilterFactory)
    {
        var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;

        ArgumentNullException.ThrowIfNull(filterFactory);

        var channelFactoryLogger = loggerFactory.CreateLogger(nameof(KestrelSocketTransportFactory));
        channelOptions.Logger = loggerFactory.CreateLogger(nameof(IChannel));

        var creator = new KestrelQuicTransportFactory(
            options: options,
            socketTransportFactory: listenerFactory,
            logger: channelFactoryLogger,
            channelFactory: connectionContext =>
            {
                var filter = filterFactory.Create(connectionContext);

                var channel = new KestrelPipeChannel<TPackageInfo>(connectionContext, filter, channelOptions);

                return new ValueTask<IChannel>(channel);
            });

        return creator;
    }
}