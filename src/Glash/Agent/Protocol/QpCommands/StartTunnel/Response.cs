using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Agent.Protocol.QpCommands.StartTunnel
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => AgentStartTunnelCommandSerializerContext.Default.Response;
    }
}
