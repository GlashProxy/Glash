using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.Login
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => LoginCommandSerializerContext.Default.Response;
    }
}
