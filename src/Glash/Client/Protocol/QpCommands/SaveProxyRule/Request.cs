using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.SaveProxyRule
{
    [DisplayName("Save Proxy Rule")]
    [Description("Add: Id is null,Update: Id is not null")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => SaveProxyRuleCommandSerializerContext.Default.Request;
        public QpModel.ProxyRuleInfo Data { get; set; }
    }
}
