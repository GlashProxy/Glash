using Glash.Core;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Glash.Server
{
    public class GlashServer
    {
        private QpServerOptions qpServerOptions;
        private QpServer qpServer;
        private CommandExecuterManager commandExecuterManager = new CommandExecuterManager();
        private NoticeHandlerManager noticeHandlerManager = new NoticeHandlerManager();
        private Dictionary<string, GlashAgentContext> agentDict = new Dictionary<string, GlashAgentContext>();
        private Dictionary<string, GlashClientContext> clientDict = new Dictionary<string, GlashClientContext>();
        private Dictionary<int, GlashServerTunnelContext> serverTunnelContextDict = new Dictionary<int, GlashServerTunnelContext>();
        private int nextTunnelId = 0;

        public GlashAgentContext[] Agents { get; private set; } = new GlashAgentContext[0];
        public GlashClientContext[] Clients { get; private set; } = new GlashClientContext[0];
        public GlashServerTunnelContext[] Tunnels { get; private set; } = new GlashServerTunnelContext[0];

        public event EventHandler<string> LogPushed;

        public event EventHandler<GlashServerTunnelContext> TunnelCreated;
        public event EventHandler<GlashServerTunnelContext> TunnelClosed;

        public event EventHandler<GlashClientContext> ClientConnected;
        public event EventHandler<GlashAgentContext> AgentConnected;

        public event EventHandler<GlashClientContext> ClientDisconnected;
        public event EventHandler<GlashAgentContext> AgentDisconnected;

        private GlashServerOptions options;

        public GlashServer(GlashServerOptions options)
        {
            this.options = options;
            commandExecuterManager.Register(
                new Glash.Agent.Protocol.QpCommands.Login.Request(),
                ExecuteCommand_Agent_Login);
            commandExecuterManager.Register(
                new Glash.Client.Protocol.QpCommands.Login.Request(),
                ExecuteCommand_Client_Login);
            commandExecuterManager.Register(
                new Glash.Client.Protocol.QpCommands.GetAgentList.Request(),
                ExecuteCommand_Client_GetAgentList);
            commandExecuterManager.Register(
                new Glash.Client.Protocol.QpCommands.GetProxyRuleList.Request(),
                ExecuteCommand_Client_GetProxyRuleList);
            commandExecuterManager.Register(
                new Glash.Client.Protocol.QpCommands.SaveProxyRule.Request(),
                ExecuteCommand_Client_SaveProxyRule);
            commandExecuterManager.Register(
                new Glash.Client.Protocol.QpCommands.DeleteProxyRule.Request(),
                ExecuteCommand_Client_DeleteProxyRule);
            commandExecuterManager.Register(
                new Glash.Client.Protocol.QpCommands.CreateTunnel.Request(),
                ExecuteCommand_Client_CreateTunnel);
            commandExecuterManager.Register(
                new Glash.Client.Protocol.QpCommands.StartTunnel.Request(),
                ExecuteCommand_Client_StartTunnel);

            noticeHandlerManager.Register<G.D>(OnTunnelDataAviliable);
            noticeHandlerManager.Register<TunnelClosed>(OnTunnelClosed);
        }

        public void Start()
        {
            if (qpServer == null)
                throw new ApplicationException("Must call Init method before start.");
            qpServer.Start();
        }

        public void Stop()
        {
            qpServer?.Stop();
        }

        public void Init(QpServer qpServer = null)
        {
            if (qpServer == null)
                qpServer = qpServerOptions.CreateServer();
            this.qpServer = qpServer;
        }

        public void HandleServerOptions(QpServerOptions qpServerOptions)
        {
            qpServerOptions.InstructionSet = new[]
            {
                Glash.Agent.Protocol.Instruction.Instance,
                Glash.Client.Protocol.Instruction.Instance
            };
            qpServerOptions.RegisterCommandExecuterManager(commandExecuterManager);
            qpServerOptions.RegisterNoticeHandlerManager(noticeHandlerManager);
            this.qpServerOptions = qpServerOptions;
        }

        //Login as Agent
        private Agent.Protocol.QpCommands.Login.Response ExecuteCommand_Agent_Login(
            QpChannel channel,
            Agent.Protocol.QpCommands.Login.Request request)
        {
            if (options.AgentManager != null)
            {
                var loginInfo = new LoginInfo()
                {
                    Name = request.Name,
                    Question = channel.AuthenticateQuestion,
                    Answer = request.Answer
                };
                if (!options.AgentManager.Login(loginInfo))
                    throw new ApplicationException("Agent authenticate failed.");
            }
            var key = request.Name;
            GlashAgentContext agent = null;
            lock (agentDict)
            {
                if (agentDict.ContainsKey(key))
                    throw new ApplicationException($"Agent [{key}] already login.");
                agent = new GlashAgentContext(key, channel);
                agentDict[key] = agent;
                Agents = agentDict.Values.ToArray();
            }
            channel.Tag = agent;
            channel.Disconnected += (s, e) =>
            {
                GlashAgentContext context = null;
                lock (agentDict)
                {
                    if (!agentDict.TryGetValue(key, out context))
                        return;
                    agentDict.Remove(key);
                    Agents = agentDict.Values.ToArray();
                }
                try
                {
                    context.Dispose();
                    foreach (var tunnel in Tunnels)
                    {
                        if (tunnel.Agent.Name == key)
                            tunnel.OnError(new ApplicationException($"Agent[{key}] disconnected."));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ExecuteCommand_Agent_Login->channel.Disconnected->context.Dispose or tunnel.OnError:" + ex.ToString());
                }
                AgentDisconnected?.Invoke(this, context);
            };
            AgentConnected?.Invoke(this, agent);
            return new Agent.Protocol.QpCommands.Login.Response();
        }

        //Login as Client
        private Client.Protocol.QpCommands.Login.Response ExecuteCommand_Client_Login(
            QpChannel channel,
            Client.Protocol.QpCommands.Login.Request request)
        {
            if (options.ClientManager != null)
            {
                var loginInfo = new LoginInfo()
                {
                    Name = request.Name,
                    Question = channel.AuthenticateQuestion,
                    Answer = request.Answer
                };
                if (!options.ClientManager.Login(loginInfo))
                    throw new ApplicationException("Client authenticate failed.");
            }
            var key = request.Name;
            GlashClientContext client = null;
            lock (clientDict)
            {
                if (clientDict.ContainsKey(key))
                    throw new ApplicationException($"Client [{key}] already login.");
                client = new GlashClientContext(key, channel);
                clientDict[key] = client;
                Clients = clientDict.Values.ToArray();
            }
            channel.Tag = client;
            channel.Disconnected += (s, e) =>
            {
                GlashClientContext context = null;
                lock (clientDict)
                {
                    if (!clientDict.TryGetValue(key, out context))
                        return;
                    clientDict.Remove(key);
                    Clients = clientDict.Values.ToArray();
                }
                try
                {
                    context.Dispose();
                    foreach (var tunnel in Tunnels)
                    {
                        if (tunnel.Client.Name == key)
                            tunnel.OnError(new ApplicationException($"Agent[{key}] disconnected."));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ExecuteCommand_Client_Login->channel.Disconnected->context.Dispose or tunnel.OnError:" + ex.ToString());
                }
                ClientDisconnected?.Invoke(this, context);
            };
            ClientConnected?.Invoke(this, client);
            return new Client.Protocol.QpCommands.Login.Response();
        }

        private Client.Protocol.QpCommands.GetAgentList.Response ExecuteCommand_Client_GetAgentList(
            QpChannel channel,
            Client.Protocol.QpCommands.GetAgentList.Request request)
        {
            var client = channel.Tag as GlashClientContext;
            if (client == null)
                throw new ApplicationException("Client not login.");
            Client.Protocol.QpModel.AgentInfo[] agents = null;
            if (options.ClientManager == null)
                agents = Agents.Select(t => new Client.Protocol.QpModel.AgentInfo()
                {
                    AgentName = t.Name,
                    IsLoggedIn = true
                }).ToArray();
            else
                agents = options.ClientManager.GetClientRelateAgents(client.Name);
            return new Client.Protocol.QpCommands.GetAgentList.Response()
            {
                Data = agents
            };
        }

        private Glash.Client.Protocol.QpCommands.GetProxyRuleList.Response ExecuteCommand_Client_GetProxyRuleList(
            QpChannel channel,
            Glash.Client.Protocol.QpCommands.GetProxyRuleList.Request request)
        {
            var client = channel.Tag as GlashClientContext;
            if (client == null)
                throw new ApplicationException("Client not login.");

            return new Client.Protocol.QpCommands.GetProxyRuleList.Response()
            {
                Data = options.ClientManager.GetProxyRuleList(client.Name, request.Agent)
            };
        }

        private Glash.Client.Protocol.QpCommands.SaveProxyRule.Response ExecuteCommand_Client_SaveProxyRule(
            QpChannel channel,
            Glash.Client.Protocol.QpCommands.SaveProxyRule.Request request)
        {
            var client = channel.Tag as GlashClientContext;
            if (client == null)
                throw new ApplicationException("Client not login.");

            return new Client.Protocol.QpCommands.SaveProxyRule.Response()
            {
                Data = options.ClientManager.SaveProxyRule(client.Name, request.Data)
            };
        }

        private Glash.Client.Protocol.QpCommands.DeleteProxyRule.Response ExecuteCommand_Client_DeleteProxyRule(
            QpChannel channel,
            Glash.Client.Protocol.QpCommands.DeleteProxyRule.Request request)
        {
            var client = channel.Tag as GlashClientContext;
            if (client == null)
                throw new ApplicationException("Client not login.");
            options.ClientManager.DeleteProxyRule(client.Name, request.ProxyRuleId);
            return new Client.Protocol.QpCommands.DeleteProxyRule.Response();
        }

        private Client.Protocol.QpCommands.CreateTunnel.Response ExecuteCommand_Client_CreateTunnel(
            QpChannel channel,
            Client.Protocol.QpCommands.CreateTunnel.Request request)
        {
            var clientContext = channel.Tag as GlashClientContext;
            if (clientContext == null)
                throw new ApplicationException("Client not login.");

            if (options.ClientManager == null)
                throw new ApplicationException("options.ClientManager is null.");

            var proxyRule = options.ClientManager.GetProxyRule(clientContext.Name, request.ProxyRuleId);
            if (proxyRule == null)
                throw new ApplicationException($"ProxyRule[Id:{request.ProxyRuleId}] not found.");

            var tunnelInfo = new TunnelInfo()
            {
                Agent = proxyRule.Agent,
                Host = proxyRule.RemoteHost,
                Port = proxyRule.RemotePort
            };
            lock (serverTunnelContextDict)
            {
                if (serverTunnelContextDict.Count >= options.MaxTunnelCount)
                    throw new ApplicationException($"Current tunnel count({serverTunnelContextDict.Count}) reach max tunnel count({options.MaxTunnelCount}).");
                var tunnelId = nextTunnelId;
                while (true)
                {
                    if (!serverTunnelContextDict.ContainsKey(tunnelId))
                        break;
                    tunnelId++;
                    if (tunnelId >= options.MaxTunnelCount)
                        tunnelId = 0;
                }
                nextTunnelId = tunnelId + 1;
                if (nextTunnelId >= options.MaxTunnelCount)
                    nextTunnelId = 0;
                tunnelInfo.Id = tunnelId;
            }
            GlashAgentContext agentContext = null;
            if (!agentDict.TryGetValue(tunnelInfo.Agent, out agentContext))
                throw new ArgumentException($"Agent[{tunnelInfo.Agent}] not login.");
            agentContext.CreateTunnelAsync(tunnelInfo).Wait();

            var tunnel = new GlashServerTunnelContext(
                tunnelInfo,
                clientContext,
                agentContext,
                ex =>
                {
                    GlashServerTunnelContext serverTunnelContext;
                    lock (serverTunnelContextDict)
                    {
                        if (!serverTunnelContextDict.TryGetValue(tunnelInfo.Id, out serverTunnelContext))
                            return;
                        serverTunnelContextDict.Remove(tunnelInfo.Id);
                        Tunnels = serverTunnelContextDict.Values.ToArray();
                    }
                    serverTunnelContext.Dispose();
                });

            lock (serverTunnelContextDict)
            {
                serverTunnelContextDict[tunnelInfo.Id] = tunnel;
                Tunnels = serverTunnelContextDict.Values.ToArray();
            }
            TunnelCreated?.Invoke(this, tunnel);
            return new Client.Protocol.QpCommands.CreateTunnel.Response() { Data = tunnelInfo };
        }

        private Client.Protocol.QpCommands.StartTunnel.Response ExecuteCommand_Client_StartTunnel(
            QpChannel channel,
            Client.Protocol.QpCommands.StartTunnel.Request request)
        {
            var clientContext = channel.Tag as GlashClientContext;
            if (clientContext == null)
                throw new ApplicationException("Client not login.");

            var tunnelId = request.TunnelId;
            GlashServerTunnelContext serverTunnelContext;
            if (!serverTunnelContextDict.TryGetValue(tunnelId, out serverTunnelContext))
                throw new ArgumentException($"Tunnel[{tunnelId}] not exist.");
            if (serverTunnelContext.Client != clientContext)
                throw new ArgumentException($"Tunnel[{tunnelId}] client context not match.");
            serverTunnelContext.StartAgentTunnel();
            return new Client.Protocol.QpCommands.StartTunnel.Response();
        }

        private void OnTunnelDataAviliable(QpChannel channel, G.D data)
        {
            if (channel.Tag == null)
                return;
            var tunnelId = data.TunnelId;
            GlashServerTunnelContext tunnel;
            if (!serverTunnelContextDict.TryGetValue(tunnelId, out tunnel))
                return;
            if (channel.Tag is GlashAgentContext agentContext)
            {
                if (tunnel.Agent != agentContext)
                    return;
                tunnel.PushDataToClient(data.Data);
            }
            else if (channel.Tag is GlashClientContext clientContext)
            {
                if (tunnel.Client != clientContext)
                    return;
                tunnel.PushDataToAgent(data.Data);
            }
        }

        private void OnTunnelClosed(QpChannel channel, TunnelClosed data)
        {
            var tunnelId = data.TunnelId;
            LogPushed?.Invoke(this, $"Tunnel[{tunnelId}] closed.");

            GlashServerTunnelContext tunnel;
            if (!serverTunnelContextDict.TryGetValue(tunnelId, out tunnel))
                return;
            tunnel.OnError(new ApplicationException("Tunnel closed."));
            TunnelClosed?.Invoke(this, tunnel);
            if (channel.Tag == null)
                return;
            if (channel.Tag is GlashAgentContext agentContext)
            {
                if (tunnel.Agent != agentContext)
                    return;
                tunnel.SendTunnelClosedNoticeToClient();
            }
            else if (channel.Tag is GlashClientContext clientContext)
            {
                if (tunnel.Client != clientContext)
                    return;
                tunnel.SendTunnelClosedNoticeToAgent();
            }
        }
    }
}
