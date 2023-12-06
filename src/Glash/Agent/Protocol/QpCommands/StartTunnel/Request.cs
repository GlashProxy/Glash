using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Agent.Protocol.QpCommands.StartTunnel
{
    [DisplayName("Start Tunnel")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => AgentStartTunnelCommandSerializerContext.Default.Request;
        public int TunnelId { get; set; }
    }
}
