using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Client.Protocol.QpCommands.GetProxyRuleList
{
    [DisplayName("Get Proxy Rule List")]
    public class Request : IQpCommandRequest<Response>
    {
        public string Agent { get; set; }
    }
}
