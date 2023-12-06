using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.DeleteProxyRule
{
    [DisplayName("Delete Proxy Rule")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => DeleteProxyRuleCommandSerializerContext.Default.Request;
        public string ProxyRuleId { get; set; }
    }
}
