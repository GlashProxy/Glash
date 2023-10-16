using Glash.Client.Protocol.QpModel;
using Glash.Core;
using Glash.Server;
using Microsoft.EntityFrameworkCore;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;
using Quick.Protocol;

namespace Glash.Blazor.Server
{
    public class Global : IAgentManager, IClientManager
    {
        public static Global Instance { get; } = new Global();
        public TextManager TextManager { get; private set; }
        public GlashServer GlashServer { get; private set; }
        public QpServerOptions ServerOptions { get; private set; }

        public string ConnectionPassword
        {
            get
            {
                var password = Model.Config.GetConfig(nameof(ConnectionPassword));
                if (string.IsNullOrEmpty(password))
                {
                    password = Guid.NewGuid().ToString("N");
                    Model.Config.SetConfig(nameof(ConnectionPassword), password);
                }
                return password;
            }
            set
            {
                Model.Config.SetConfig(nameof(ConnectionPassword), value);
                ServerOptions.Password = value;
            }
        }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.Config>();
            modelBuilder.Entity<Model.AgentInfo>();
            modelBuilder.Entity<Model.ClientInfo>();
            modelBuilder.Entity<Model.ClientAgentRelation>()
                .HasKey(t => new { t.ClientName, t.AgentName });
            modelBuilder.Entity<Model.ProxyRuleInfo>();
        }

        public void ChangeLanguage(string language)
        {
            TextManager = TextManager.GetInstance(language);
        }

        public void Init(Quick.Protocol.WebSocket.Server.AspNetCore.QpWebSocketServerOptions serverOptions, int maxTunnelCount)
        {
            TextManager = TextManager.DefaultInstance;
            GlashServer = new GlashServer(new GlashServerOptions()
            {
                MaxTunnelCount = maxTunnelCount,
                AgentManager = this,
                ClientManager = this
            });
            GlashServer.AgentConnected += GlashServer_AgentConnected;
            GlashServer.AgentDisconnected += GlashServer_AgentDisconnected;
            GlashServer.ClientConnected += GlashServer_ClientConnected;
            GlashServer.ClientDisconnected += GlashServer_ClientDisconnected;
            GlashServer.HandleServerOptions(serverOptions);
        }

        private void onAgentLoginStatusChanged(string agentName, bool isLoggedIn)
        {
            var relateClients = ConfigDbContext.CacheContext
                .Query<Model.ClientAgentRelation>(t => t.AgentName == agentName)
                .Select(t => ConfigDbContext.CacheContext.Find(new Model.ClientInfo(t.ClientName)))
                .Where(t => t != null)
                .ToArray();
            foreach (var client in relateClients)
            {
                var qpChannel = client.Context?.Channel;
                if (qpChannel == null)
                    continue;
                qpChannel.SendNoticePackage(new Client.Protocol.QpNotices.AgentLoginStatusChanged()
                {
                    Data = new AgentInfo()
                    {
                        AgentName = agentName,
                        IsLoggedIn = isLoggedIn
                    }
                });
            }
        }

        private void GlashServer_AgentConnected(object sender, GlashAgentContext e)
        {
            var agentInfo = ConfigDbContext.CacheContext.Find(new Model.AgentInfo(e.Name));
            if (agentInfo == null)
                return;
            agentInfo.Context = e;
            onAgentLoginStatusChanged(e.Name, true);
        }

        private void GlashServer_AgentDisconnected(object sender, GlashAgentContext e)
        {
            var agentInfo = ConfigDbContext.CacheContext.Find(new Model.AgentInfo(e.Name));
            if (agentInfo == null)
                return;
            agentInfo.Context = null;
            onAgentLoginStatusChanged(e.Name, false);
        }

        private void GlashServer_ClientConnected(object sender, GlashClientContext e)
        {
            var clientInfo = ConfigDbContext.CacheContext.Find(new Model.ClientInfo(e.Name));
            if (clientInfo == null)
                return;
            clientInfo.Context = e;
        }

        private void GlashServer_ClientDisconnected(object sender, GlashClientContext e)
        {
            var clientInfo = ConfigDbContext.CacheContext.Find(new Model.ClientInfo(e.Name));
            if (clientInfo == null)
                return;
            clientInfo.Context = null;
        }

        bool IAgentManager.Login(LoginInfo loginInfo)
        {
            var model = ConfigDbContext.CacheContext
                        .Find(new Model.AgentInfo(loginInfo.Name));
            if (model == null)
                return false;
            var answer = CryptoUtils.GetAnswer(loginInfo.Question, model.Password);
            return answer == loginInfo.Answer;
        }

        bool IClientManager.Login(LoginInfo loginInfo)
        {
            var model = ConfigDbContext.CacheContext
                .Find(new Model.ClientInfo(loginInfo.Name));
            if (model == null)
                return false;
            var answer = CryptoUtils.GetAnswer(loginInfo.Question, model.Password);
            return answer == loginInfo.Answer;
        }

        AgentInfo[] IClientManager.GetClientRelateAgents(string clientName)
        {
            return ConfigDbContext.CacheContext
                .Query<Model.ClientAgentRelation>()
                .Where(t => t.ClientName == clientName)
                .Select(t =>
                {
                    var agentModel = ConfigDbContext.CacheContext.Find(new Model.AgentInfo(t.AgentName));
                    if (agentModel == null)
                        return null;
                    return new AgentInfo()
                    {
                        AgentName = agentModel.Name,
                        IsLoggedIn = agentModel.Context != null
                    };
                })
                .Where(t => t != null)
                .ToArray();
        }

        bool IClientManager.IsClientRelateAgent(string clientName, string agnetName)
        {
            var model = ConfigDbContext.CacheContext
                .Find(new Model.ClientAgentRelation()
                {
                    ClientName = clientName,
                    AgentName = agnetName
                });
            return model != null;
        }

        public ProxyRuleInfo[] GetProxyRuleList(string clientName, string agentName)
        {
            return ConfigDbContext.CacheContext
                .Query<Model.ProxyRuleInfo>(t =>
                    t.ClientName == clientName
                    && (agentName == null || t.Agent == agentName));
        }

        public ProxyRuleInfo SaveProxyRule(string clientName, ProxyRuleInfo proxyRule)
        {
            IClientManager manager = this;
            if (!manager.IsClientRelateAgent(clientName, proxyRule.Agent))
                throw new ApplicationException($"Client[{clientName}] not relate to Agent[{proxyRule.Agent}].");

            var model = new Model.ProxyRuleInfo()
            {
                Id = proxyRule.Id,
                Agent = proxyRule.Agent,
                ClientName = clientName,
                Enable = proxyRule.Enable,
                LocalIPAddress = proxyRule.LocalIPAddress,
                LocalPort = proxyRule.LocalPort,
                Name = proxyRule.Name,
                RemoteHost = proxyRule.RemoteHost,
                RemotePort = proxyRule.RemotePort,
                ProxyType = proxyRule.ProxyType,
                ProxyTypeConfig = proxyRule.ProxyTypeConfig
            };

            //Add
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = Guid.NewGuid().ToString("N");
                ConfigDbContext.CacheContext.Add(model);
            }
            //Update
            else
            {
                ConfigDbContext.CacheContext.Update(model);
            }
            return model;
        }

        public void DeleteProxyRule(string clientName, string proxyRuleId)
        {
            var model = ConfigDbContext.CacheContext.Find(new Model.ProxyRuleInfo(proxyRuleId));
            if (model == null)
                throw new ApplicationException($"Can't found ProxyRule with Id[{proxyRuleId}].");

            if (model.ClientName != model.ClientName)
                throw new ApplicationException($"ProxyRule[{proxyRuleId}] not belong to Client[{clientName}].");

            ConfigDbContext.CacheContext.Remove(model);
        }

        public ProxyRuleInfo GetProxyRule(string clientName, string proxyRuleId)
        {
            var model = ConfigDbContext.CacheContext.Find(new Model.ProxyRuleInfo(proxyRuleId));
            if (model == null)
                throw new ApplicationException($"Can't found ProxyRule with Id[{proxyRuleId}].");
            if(model.ClientName != clientName)
                throw new ApplicationException($"ProxyRule[Id:{proxyRuleId}]'s client name not match.");
            return model;
        }
    }
}
