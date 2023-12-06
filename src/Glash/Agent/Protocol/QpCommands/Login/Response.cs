using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Agent.Protocol.QpCommands.Login
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => AgentLoginCommandSerializerContext.Default.Response;
    }
}
