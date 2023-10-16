using Glash.Core;
using Quick.Protocol;
using System.ComponentModel;

namespace Glash.Agent.Protocol.QpCommands.CreateTunnel
{
    [DisplayName("Create Tunnel")]
    public class Request : IQpCommandRequest<Response>
    {
        public TunnelInfo Data { get; set; }
    }
}
