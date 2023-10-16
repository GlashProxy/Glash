using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Client.Protocol.QpCommands.SaveProxyRule
{
    [DisplayName("Save Proxy Rule")]
    [Description("Add: Id is null,Update: Id is not null")]
    public class Request : IQpCommandRequest<Response>
    {
        public QpModel.ProxyRuleInfo Data { get; set; }
    }
}
