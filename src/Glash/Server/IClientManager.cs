namespace Glash.Server
{
    public interface IClientManager
    {
        bool Login(LoginInfo loginInfo);
        Client.Protocol.QpModel.AgentInfo[] GetClientRelateAgents(string clientName);
        bool IsClientRelateAgent(string clientName, string agnetName);
        Client.Protocol.QpModel.ProxyRuleInfo[] GetProxyRuleList(string clientName, string agentName);
        Glash.Client.Protocol.QpModel.ProxyRuleInfo GetProxyRule(string clientName, string proxyRuleId);
        Client.Protocol.QpModel.ProxyRuleInfo SaveProxyRule(string clientName, Client.Protocol.QpModel.ProxyRuleInfo proxyRule);
        void DeleteProxyRule(string clientName, string proxyRuleId);        
    }
}
