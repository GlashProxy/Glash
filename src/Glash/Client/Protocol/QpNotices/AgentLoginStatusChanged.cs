using Glash.Client.Protocol.QpModel;
using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpNotices
{
    [DisplayName("Agent login status changed")]
    public class AgentLoginStatusChanged : AbstractQpSerializer<AgentLoginStatusChanged>
    {
        protected override JsonTypeInfo<AgentLoginStatusChanged> GetTypeInfo() => ClientNoticesSerializerContext.Default.AgentLoginStatusChanged;
        public AgentInfo Data { get; set; }
    }
}
