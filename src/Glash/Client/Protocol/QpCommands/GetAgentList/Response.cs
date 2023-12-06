using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.GetAgentList
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => GetAgentListCommandSerializerContext.Default.Response;
        public QpModel.AgentInfo[] Data { get; set; }
    }
}
