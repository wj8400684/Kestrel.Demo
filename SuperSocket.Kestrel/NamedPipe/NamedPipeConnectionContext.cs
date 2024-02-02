using System.IO.Pipelines;
using System.IO.Pipes;
using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;

namespace SuperSocket.Kestrel.NamedPipe;

internal sealed class NamedPipeConnectionContext : ConnectionContext, IDuplexPipe
{
    private readonly PipeStream _stream;

    public NamedPipeConnectionContext(PipeStream stream, EndPoint localEndPoint)
    {
        _stream = stream;
        Transport = this;
        LocalEndPoint = localEndPoint;
        RemoteEndPoint = localEndPoint;
        ConnectionId = Guid.NewGuid().ToString();

        Input = PipeReader.Create(stream);
        Output = PipeWriter.Create(stream);
    }

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

        await _stream.DisposeAsync();
    }
}