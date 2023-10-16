using Glash.Core;
using Quick.Protocol;
using System.ComponentModel;

namespace Glash.Client.Protocol.QpCommands.CreateTunnel
{
    [DisplayName("Create Tunnel")]
    public class Request : IQpCommandRequest<Response>
    {
        public string ProxyRuleId { get; set; }
    }
}
