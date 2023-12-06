using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.Login
{
    [DisplayName("Login as Client")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => LoginCommandSerializerContext.Default.Request;
        public string Name { get; set; }
        public string Answer { get; set; }
    }
}
