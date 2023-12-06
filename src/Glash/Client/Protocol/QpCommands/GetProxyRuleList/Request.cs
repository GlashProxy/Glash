using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.GetProxyRuleList
{
    [DisplayName("Get Proxy Rule List")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => GetProxyRuleListCommandSerializerContext.Default.Request;
        public string Agent { get; set; }
    }
}
