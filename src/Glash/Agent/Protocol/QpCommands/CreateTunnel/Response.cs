using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Agent.Protocol.QpCommands.CreateTunnel
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => AgentCreateTunnelCommandSerializerContext.Default.Response;
    }
}
