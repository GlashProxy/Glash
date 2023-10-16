using Quick.Protocol;
using System.ComponentModel;

namespace Glash.Client.Protocol.QpCommands.StartTunnel
{
    [DisplayName("Start Tunnel")]
    public class Request : IQpCommandRequest<Response>
    {
        public int TunnelId { get; set; }
    }
}
