using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Client.Protocol.QpCommands.GetProxyRuleList
{
    public class Response
    {
        public QpModel.ProxyRuleInfo[] Data { get; set; }
    }
}
