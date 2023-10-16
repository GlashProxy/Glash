using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Server
{
    public class GlashServerOptions
    {
        public int MaxTunnelCount { get; set; } = 100;
        public IAgentManager AgentManager { get; set; }
        public IClientManager ClientManager { get; set; }
    }
}
