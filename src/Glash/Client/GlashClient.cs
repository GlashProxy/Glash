using Quick.Protocol;
using Quick.Protocol.Utils;
using Glash.Core;
using Glash.Client.Protocol.QpModel;
using Glash.Client.Protocol.QpNotices;
using System.Reflection;

namespace Glash.Client
{
    public class GlashClient : IDisposable
    {
        private QpClient qpClient;
        private Dictionary<string, ProxyRuleContext> proxyRuleContextDict = new Dictionary<string, ProxyRuleContext>();

        public event EventHandler<AgentLoginStatusChanged> AgentLoginStatusChanged;
        public event EventHandler Disconnected;
        public event EventHandler<string> LogPushed;
        
        public ProxyRuleContext[] ProxyRuleContexts => proxyRuleContextDict.Values.ToArray();

        public GlashClient(string url, string password = null)
        {
            var qpClientOptions = QpClientOptions.Parse(new Uri(url));
            if (!string.IsNullOrEmpty(password))
                qpClientOptions.Password = password;
            qpClientOptions.InstructionSet = new[]
            {
                Client.Protocol.Instruction.Instance
            };
            var noticeHandlerManager = new NoticeHandlerManager();
            noticeHandlerManager.Register<G.D>(OnTunnelDataAviliable);
            noticeHandlerManager.Register<TunnelClosed>(OnTunnelClosed);
            noticeHandlerManager.Register<AgentLoginStatusChanged>(OnAgentLoginStatusChanged);
            qpClientOptions.RegisterNoticeHandlerManager(noticeHandlerManager);
            qpClient = qpClientOptions.CreateClient();
            qpClient.Disconnected += QpClient_Disconnected;
        }

        private void closeAllTunnel()
        {
            GlashTunnelContext[] tunnels = null;
            lock (tunnelContextDict)
            {
                tunnels = tunnelContextDict.Values.ToArray();
                tunnelContextDict.Clear();
            }
            foreach (var tunnel in tunnels)
                tunnel.Dispose();
        }

        private void QpClient_Disconnected(object sender, EventArgs e)
        {
            closeAllTunnel();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public async Task ConnectAsync(string user, string password)
        {
            //Connect
            await qpClient.ConnectAsync();
            var answer = CryptoUtils.GetAnswer(qpClient.AuthenticateQuestion, password);
            //Register
            await qpClient.SendCommand(new Protocol.QpCommands.Login.Request()
            {
                Name = user,
                Answer = answer
            });
        }

        public void Dispose()
        {
            closeAllTunnel();
            foreach (var proxyContext in proxyRuleContextDict.Values)
                proxyContext.Stop();
            proxyRuleContextDict.Clear();
            qpClient.Disconnect();
        }

        public void EnableProxyRule(ProxyRuleContext context)
        {
            try
            {
                context.Start();
                context.Config.Enable = true;
                LogPushed?.Invoke(this, $"{context.Config} enabled.");
            }
            catch (Exception ex)
            {
                context.Config.Enable = false;
                LogPushed?.Invoke(this, $"{context.Config} enable failed.Reason:{ExceptionUtils.GetExceptionMessage(ex)}");
                throw;
            }
        }

        public void EnableProxyRule(string proxyRuleId)
        {
            if (!proxyRuleContextDict.ContainsKey(proxyRuleId))
                return;
            var context = proxyRuleContextDict[proxyRuleId];
            EnableProxyRule(context);
        }

        public void DisableProxyRule(ProxyRuleContext context)
        {
            try
            {
                context.Stop();
                context.Config.Enable = false;
                LogPushed?.Invoke(this, $"{context.Config} disabled.");
            }
            catch (Exception ex)
            {
                LogPushed?.Invoke(this, $"{context.Config} disable failed.Reason:{ExceptionUtils.GetExceptionMessage(ex)}");
                throw;
            }
        }

        public void DisableProxyRule(string proxyRuleId)
        {
            if (!proxyRuleContextDict.ContainsKey(proxyRuleId))
                return;
            var context = proxyRuleContextDict[proxyRuleId];
            DisableProxyRule(context);
        }

        public void LoadProxyRule(ProxyRuleInfo config)
        {
            var context = new ProxyRuleContext(this, config);
            proxyRuleContextDict[config.Id] = context;
            if (config.Enable)
                try
                {
                    EnableProxyRule(context);
                }
                catch { }
        }

        public void LoadProxyRules(ProxyRuleInfo[] items)
        {
            foreach (var item in items)
                LoadProxyRule(item);
        }

        public void UnloadProxyRule(ProxyRuleContext proxyRuleContext)
        {
            if (proxyRuleContextDict.ContainsKey(proxyRuleContext.Config.Id))
                proxyRuleContextDict.Remove(proxyRuleContext.Config.Id);
            DisableProxyRule(proxyRuleContext);
        }

        public void UnloadProxyRule(string proxyRuleId)
        {
            if (!proxyRuleContextDict.ContainsKey(proxyRuleId))
                return;
            UnloadProxyRule(proxyRuleContextDict[proxyRuleId]);
        }

        private Dictionary<int, GlashTunnelContext> tunnelContextDict = new Dictionary<int, GlashTunnelContext>();

        internal async Task CreateAndStartTunnelAsync(ProxyRuleInfo config, string connectionName, Stream stream)
        {
            try
            {
                //Create Tunnel
                var rep = await qpClient.SendCommand(new Protocol.QpCommands.CreateTunnel.Request()
                {
                    ProxyRuleId = config.Id
                });
                var tunnelId = rep.Data.Id;
                var tunnelContext = new GlashTunnelContext(
                    qpClient,
                    rep.Data,
                    stream,
                    ex =>
                    {
                        LogPushed?.Invoke(this, $"Tunnel[{tunnelId}] error.Message:{ExceptionUtils.GetExceptionMessage(ex)}");
                        qpClient.SendNoticePackage(new TunnelClosed() { TunnelId = tunnelId });

                        GlashTunnelContext tunnelContext = null;
                        lock (tunnelContextDict)
                        {
                            if (!tunnelContextDict.ContainsKey(tunnelId))
                                return;
                            tunnelContext = tunnelContextDict[tunnelId];
                        }
                        tunnelContext.Dispose();
                        LogPushed?.Invoke(this, $"Tunnel[{tunnelId}] closed.");
                    });
                lock (tunnelContextDict)
                    tunnelContextDict[tunnelId] = tunnelContext;

                //Start Tunnel
                await qpClient.SendCommand(new Protocol.QpCommands.StartTunnel.Request() { TunnelId = tunnelId });
                tunnelContext.Start();

                LogPushed?.Invoke(this, $"[{connectionName}]: Create tunnel[{tunnelId}] to tcp://{config.Agent}/{config.RemoteHost}/{config.RemotePort} success.");
            }
            catch (Exception ex)
            {
                LogPushed?.Invoke(this, $"[{connectionName}]: Create tunnel to tcp://{config.Agent}/{config.RemoteHost}/{config.RemotePort} failed.Reason:{ExceptionUtils.GetExceptionMessage(ex)}");
                try
                {
                    stream.Close();
                    stream.Dispose();
                }
                catch { }
            }
        }

        private void OnTunnelDataAviliable(QpChannel channel, G.D data)
        {
            var tunnelId = data.TunnelId;
            if (!tunnelContextDict.ContainsKey(tunnelId))
                return;
            var tunnelContext = tunnelContextDict[tunnelId];
            tunnelContext.PushData(data.Data);
        }

        private void OnTunnelClosed(QpChannel channel, TunnelClosed data)
        {
            var tunnelId = data.TunnelId;
            GlashTunnelContext tunnelContext = null;
            lock (tunnelContextDict)
            {
                if (!tunnelContextDict.ContainsKey(tunnelId))
                    return;
                tunnelContext = tunnelContextDict[tunnelId];
                tunnelContextDict.Remove(tunnelId);
            }
            tunnelContext.Dispose();
            LogPushed?.Invoke(this, $"Tunnel[{tunnelId}] closed.");
        }

        private void OnAgentLoginStatusChanged(QpChannel channel, AgentLoginStatusChanged data)
        {
            AgentLoginStatusChanged?.Invoke(this, data);
        }

        public async Task<AgentInfo[]> GetAgentListAsync()
        {
            var rep = await qpClient.SendCommand(new Protocol.QpCommands.GetAgentList.Request());
            return rep.Data;
        }

        public async Task<ProxyRuleInfo[]> GetProxyRuleListAsync()
        {
            var rep = await qpClient.SendCommand(new Protocol.QpCommands.GetProxyRuleList.Request());
            return rep.Data;
        }

        public async Task<ProxyRuleInfo> SaveProxyRule(ProxyRuleInfo model)
        {
            var rep = await qpClient.SendCommand(new Glash.Client.Protocol.QpCommands.SaveProxyRule.Request()
            {
                Data = model
            });
            return rep.Data;
        }

        public async Task DeleteProxyRule(string proxyRuleId)
        {
            await qpClient.SendCommand(new Glash.Client.Protocol.QpCommands.DeleteProxyRule.Request()
            {
                ProxyRuleId = proxyRuleId
            });
        }

        public void DisableAgentProxyRules(string agentName)
        {
            foreach (var context in ProxyRuleContexts)
            {
                if (context.Config.Agent != agentName)
                    continue;
                if (!context.Config.Enable)
                    continue;
                DisableProxyRule(context);
            }
        }
    }
}