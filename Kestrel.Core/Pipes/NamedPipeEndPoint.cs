using System.Net;

namespace Bedrock.Framework
{
    public sealed class NamedPipeEndPoint(string pipeName, string serverName = ".",
            System.IO.Pipes.PipeOptions pipeOptions = System.IO.Pipes.PipeOptions.Asynchronous)
        : EndPoint
    {
        public string ServerName { get; } = serverName;
        public string PipeName { get; } = pipeName;
        public System.IO.Pipes.PipeOptions PipeOptions { get; set; } = pipeOptions;

        public override string ToString()
        {
            return $"Server = {ServerName}, Pipe = {PipeName}";
        }
    }
}
