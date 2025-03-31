using Glash.Client;
using Glash.Client.Protocol.QpModel;

namespace Glash.Blazor.Client;

public class ProfileContext
{
    public static int MaxLogLines = 1000;

    public Model.Profile Profile { get; private set; }
    public GlashClient GlashClient { get; private set; }
    public AgentInfo[] Agents { get; private set; }
    public ProxyRuleInfo[] ProxyRules { get; private set; }
    private Queue<string> logQueue = new Queue<string>();
    public string[] Logs
    {
        get
        {
            lock (logQueue)
                return logQueue.ToArray();
        }
    }

    private Dictionary<string, AgentInfo> agentDict;

    private bool _Enabled;
    public bool Enabled
    {
        get
        {
            return _Enabled;
        }
        private set
        {
            _Enabled = value;
            EnableStateChanged?.Invoke(this, value);
        }
    }
    public EventHandler<bool> EnableStateChanged;
    public EventHandler<AgentInfo> AgentLoginStatusChanged;
    public EventHandler<string[]> LogChanged;

    public ProfileContext(Model.Profile profile)
    {
        Profile = profile;
    }

    public async Task Enable()
    {
        try
        {
            GlashClient = new GlashClient(Profile.ServerUrl);
            await GlashClient.ConnectAsync(Profile.ClientName, Profile.ClientPassword);
            var agentList = await GlashClient.GetAgentListAsync();
            Agents = agentList
                .OrderBy(t => t.AgentName)
                .OrderByDescending(t => t.IsLoggedIn)
                .ToArray();
            agentDict = Agents.ToDictionary(t => t.AgentName, t => t);

            var proxyRuleList = await GlashClient.GetProxyRuleListAsync();
            ProxyRules = proxyRuleList
                .OrderBy(t => t.Name)
                .ToArray();
            GlashClient.LoadProxyRules(proxyRuleList);

            Enabled = true;
            GlashClient.AgentLoginStatusChanged += GlashClient_AgentLoginStatusChanged;
            GlashClient.LogPushed += GlashClient_LogPushed;
            GlashClient.Disconnected += GlashClient_Disconnected;
        }
        catch
        {
            GlashClient?.Dispose();
            GlashClient = null;
            throw;
        }
    }

    public async Task Disable()
    {
        await Task.Run(() =>
        {
            if (GlashClient != null)
            {
                foreach (var proxyRuleContext in GlashClient.ProxyRuleContexts)
                    GlashClient.UnloadProxyRule(proxyRuleContext);
                GlashClient.AgentLoginStatusChanged -= GlashClient_AgentLoginStatusChanged;
                GlashClient.LogPushed -= GlashClient_LogPushed;
                GlashClient.Disconnected -= GlashClient_Disconnected;
                GlashClient.Dispose();
                GlashClient = null;
                Enabled = false;
            }
        });
    }



    private void GlashClient_AgentLoginStatusChanged(
        object sender,
        Glash.Client.Protocol.QpNotices.AgentLoginStatusChanged e)
    {
        Task.Run(() =>
        {
            var data = e.Data;
            if (!agentDict.ContainsKey(data.AgentName))
                return;
            var agentInfo = agentDict[data.AgentName];
            agentInfo.IsLoggedIn = data.IsLoggedIn;
            if (!data.IsLoggedIn)
                GlashClient.DisableAgentProxyRules(agentInfo.AgentName);
            AgentLoginStatusChanged?.Invoke(this, data);
        });
    }

    private void GlashClient_LogPushed(object sender, string e)
    {
        var line = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {e}";
        lock (logQueue)
        {
            logQueue.Enqueue(line);
            while (true)
            {
                var currentCount = logQueue.Count;
                if (currentCount == 0 || currentCount <= MaxLogLines)
                    break;
                logQueue.Dequeue();
            }
        }
        LogChanged?.Invoke(this, Logs);
    }

    private void GlashClient_Disconnected(object sender, EventArgs e)
    {
        _ = Disable();
    }

    public async Task AddProxyRule(ProxyRuleInfo model)
    {
        model = await GlashClient.SaveProxyRule(model);
        GlashClient.LoadProxyRule(model);
    }

    public async Task DuplicateProxyRule(ProxyRuleInfo newModel)
    {
        newModel = await GlashClient.SaveProxyRule(newModel);
        GlashClient.LoadProxyRule(newModel);
    }

    public async Task EditProxyRule(ProxyRuleInfo model)
    {
        GlashClient.UnloadProxyRule(model.Id);
        model = await GlashClient.SaveProxyRule(model);
        GlashClient.LoadProxyRule(model);
    }

    public async Task DeleteProxyRule(ProxyRuleInfo model)
    {
        GlashClient.UnloadProxyRule(model.Id);
        await GlashClient.DeleteProxyRule(model.Id);
    }
}
