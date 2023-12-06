using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.DeleteProxyRule
{
    public class Response: AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => DeleteProxyRuleCommandSerializerContext.Default.Response;
    }
}
