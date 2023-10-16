using Glash.Core;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Server
{
    public class GlashAgentContext : IDisposable
    {
        public string Name { get; private set; }
        public QpChannel Channel { get; private set; }
        public DateTime CreateTime { get; private set; }
        public GlashAgentContext(string name, QpChannel channel)
        {
            Name = name;
            Channel = channel;
            CreateTime = DateTime.Now;
        }

        public void Dispose()
        {
            try
            {
                Channel.Disconnect();
            }
            catch { }
        }

        public async Task CreateTunnelAsync(TunnelInfo tunnelInfo)
        {
            await Channel.SendCommand(new Glash.Agent.Protocol.QpCommands.CreateTunnel.Request()
            {
                Data = tunnelInfo
            });
        }

        public async Task StartTunnelAsync(int tunnelId)
        {
            await Channel.SendCommand(new Glash.Agent.Protocol.QpCommands.StartTunnel.Request()
            {
                TunnelId = tunnelId
            });
        }
    }
}
