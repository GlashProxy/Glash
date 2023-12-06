using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.SaveProxyRule
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => SaveProxyRuleCommandSerializerContext.Default.Response;
        public QpModel.ProxyRuleInfo Data { get; set; }
    }
}
