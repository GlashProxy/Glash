using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace Glash.Client.Protocol.QpCommands.StartTunnel
{
    [DisplayName("Start Tunnel")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => StartTunnelCommandSerializerContext.Default.Request;
        public int TunnelId { get; set; }
    }
}
