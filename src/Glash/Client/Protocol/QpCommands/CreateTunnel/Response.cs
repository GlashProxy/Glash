using Glash.Core;
using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.CreateTunnel
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => CreateTunnelCommandSerializerContext.Default.Response;
        public TunnelInfo Data { get; set; }
    }
}
