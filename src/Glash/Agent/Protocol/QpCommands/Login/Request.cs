using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Agent.Protocol.QpCommands.Login
{
    [DisplayName("Login as Agent")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request,Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => AgentLoginCommandSerializerContext.Default.Request;
        public string Name { get; set; }
        public string Answer { get; set; }
    }
}
