using Glash.Client.Protocol.QpModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Client.Protocol.QpNotices
{
    [DisplayName("Agent login status changed")]
    public class AgentLoginStatusChanged
    {
        public AgentInfo Data { get; set; }
    }
}
