using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.GetProxyRuleList
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => GetProxyRuleListCommandSerializerContext.Default.Response;
        public QpModel.ProxyRuleInfo[] Data { get; set; }
    }
}
