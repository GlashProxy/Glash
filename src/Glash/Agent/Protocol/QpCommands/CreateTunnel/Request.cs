using Glash.Core;
using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Agent.Protocol.QpCommands.CreateTunnel
{
    [DisplayName("Create Tunnel")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => AgentCreateTunnelCommandSerializerContext.Default.Request;
        public TunnelInfo Data { get; set; }
    }
}
