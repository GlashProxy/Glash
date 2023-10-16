using Quick.Protocol;
using System.ComponentModel;

namespace Glash.Agent.Protocol.QpCommands.StartTunnel
{
    [DisplayName("Start Tunnel")]
    public class Request : IQpCommandRequest<Response>
    {
        public int TunnelId { get; set; }
    }
}
