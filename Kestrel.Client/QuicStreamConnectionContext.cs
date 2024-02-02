using System.IO.Pipelines;
using System.IO.Pipes;
using System.Net;
using System.Net.Quic;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;

namespace Kestrel.Client;

public class QuicStreamConnectionContext : ConnectionContext, IDuplexPipe
{
#pragma warning disable CA2252
    private readonly QuicStream _outgoingStream;
    private readonly QuicStream _inboundStream;
    
    public QuicStreamConnectionContext(QuicStream outgoingStream, QuicStream inboundStream, EndPoint remoteEndPoint,
        EndPoint localEndPoint)
    {
        _outgoingStream = outgoingStream;
        _inboundStream = inboundStream;
        Transport = this;
        ConnectionId = Guid.NewGuid().ToString();
        RemoteEndPoint = remoteEndPoint;
        LocalEndPoint = localEndPoint;
        Input = PipeReader.Create(_inboundStream);
        Output = PipeWriter.Create(_outgoingStream);
    }

#pragma warning restore CA2252

    public PipeReader Input { get; }

    public PipeWriter Output { get; }
    public override string ConnectionId { get; set; }

    public override IFeatureCollection Features { get; } = new FeatureCollection();

    public override IDictionary<object, object> Items { get; set; } = new ConnectionItems();
    public override IDuplexPipe Transport { get; set; }

    public override void Abort()
    {
        // TODO: Abort the named pipe. Do we dispose the Stream?
        base.Abort();
    }

    public override async ValueTask DisposeAsync()
    {
        Input.Complete();
        Output.Complete();
#pragma warning disable CA2252
        await _inboundStream.DisposeAsync();
        await _outgoingStream.DisposeAsync();
#pragma warning restore CA2252
    }
}