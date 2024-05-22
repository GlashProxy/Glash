using Glash.Client;
using Glash.Client.Protocol.QpModel;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Localize;

namespace Glash.Blazor.Client
{
    public partial class Main
    {
        [Parameter]
        public INavigator INavigator { get; set; }
        [Parameter]
        public Model.Profile CurrentProfile { get; set; }
        [Parameter]
        public GlashClient GlashClient { get; set; }
        [Parameter]
        public AgentInfo[] Agents { get; set; }
        [Parameter]
        public ProxyRuleInfo[] ProxyRules { get; set; }


        private static string TextAddProxyRule=>Locale.GetString("Add Proxy Rule");
        private static string TextDuplicateProxyRule => Locale.GetString("Duplicate Proxy Rule");
        private static string TextEditProxyRule => Locale.GetString("Edit Proxy Rule");
        private static string TextDeleteProxyRule => Locale.GetString("Delete Proxy Rule");
        private static string TextLogout => Locale.GetString("Logout");
        private static string TextEnableAll => Locale.GetString("Enable All");
        private static string TextDisableAll => Locale.GetString("Disable All");
        private static string TextSystem => Locale.GetString("System");
        private static string TextLocal => Locale.GetString("Local");
        private static string TextRemote => Locale.GetString("Remote");
        private static string TextDisplayRows => Locale.GetString("Display Rows");
        private static string TextDisconnectedFromServer => Locale.GetString("Disconnected from server");
        private static string TextAgentNotLogin => Locale.GetString("Agent not login");
        private static string TextError => Locale.GetString("Error");
        private Dictionary<string, AgentInfo> agentDict;

        private bool isUserLogout = false;
        private ModalAlert modalAlert;
        private ModalWindow modalWindow;
        private ModalLoading modalLoading;
        private ModalPrompt modalPrompt;
        private LogViewControl logViewControl;

        private Queue<string> logQueue = new Queue<string>();
        private string Logs;
        private int LogRows = 25;
        private int MAX_LOG_LINES = 1000;

        public static Dictionary<string, object> PrepareParameter(
            Model.Profile currentProfile,
            GlashClient glashClient,
            AgentInfo[] agents,
            ProxyRuleInfo[] proxyRules)
        {
            return new Dictionary<string, object>()
            {
                [nameof(CurrentProfile)] = currentProfile,
                [nameof(GlashClient)] = glashClient,
                [nameof(Agents)] = agents,
                [nameof(ProxyRules)] = proxyRules
            };
        }

        protected override void OnParametersSet()
        {
            agentDict = Agents.ToDictionary(t => t.AgentName, t => t);
            GlashClient.AgentLoginStatusChanged += GlashClient_AgentLoginStatusChanged;
            GlashClient.LogPushed += GlashClient_LogPushed;
            GlashClient.Disconnected += GlashClient_Disconnected;
            var agentHashSet = Agents.Select(t => t.AgentName).ToHashSet();
            GlashClient.LoadProxyRules(ProxyRules);
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
                InvokeAsync(StateHasChanged);
            });
        }

        private void GlashClient_LogPushed(object sender, string e)
        {
            var line = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {e}";
            lock (logQueue)
            {
                logQueue.Enqueue(line);
                while (logQueue.Count > MAX_LOG_LINES)
                    logQueue.Dequeue();
                Logs = string.Join(Environment.NewLine, logQueue);
            }
            logViewControl?.SetContent(Logs);
        }

        private void GlashClient_Disconnected(object sender, EventArgs e)
        {
            if (isUserLogout)
                return;
            INavigator.Alert(
                TextError,
                TextDisconnectedFromServer);
            logout();
        }

        private void logout()
        {
            isUserLogout = true;
            if (GlashClient != null)
            {
                foreach (var proxyRuleContext in GlashClient.ProxyRuleContexts)
                    GlashClient.UnloadProxyRule(proxyRuleContext);
                GlashClient.AgentLoginStatusChanged -= GlashClient_AgentLoginStatusChanged;
                GlashClient.LogPushed -= GlashClient_LogPushed;
                GlashClient.Disconnected -= GlashClient_Disconnected;
                GlashClient.Dispose();
                GlashClient = null;
            }
            Global.Instance.Profile = null;
            INavigator.Navigate<Login>();
        }

        private void Logout()
        {
            modalAlert.Show(
                TextLogout,
                Locale.GetString("Are you sure to logout?"),
                () =>
                {
                    logout();
                }
            );
        }

        private void AddProxyRule(string agent)
        {
            modalWindow.Show<Controls.EditProxyRule>(TextAddProxyRule, Controls.EditProxyRule.PrepareParameter(
                new ProxyRuleInfo()
                {
                    LocalIPAddress = "127.0.0.1",
                    LocalPort = 80,
                    RemoteHost = "127.0.0.1",
                    RemotePort = 80,
                    Agent = agent
                },
                async model =>
                {
                    modalLoading.Show(TextAddProxyRule, null, true);
                    try
                    {
                        model = await GlashClient.SaveProxyRule(model);
                        GlashClient.LoadProxyRule(model);
                        _ = InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(TextError, ex.Message);
                    }
                    modalLoading.Close();
                }
            ));
        }

        private void DuplicateProxyRule(ProxyRuleInfo model)
        {
            modalPrompt.Show(TextDuplicateProxyRule, model.Name, async newName =>
            {
                var newModel = new ProxyRuleInfo()
                {
                    Name = newName,
                    Agent = model.Agent,
                    LocalIPAddress = model.LocalIPAddress,
                    LocalPort = model.LocalPort,
                    RemoteHost = model.RemoteHost,
                    RemotePort = model.RemotePort,
                    ProxyType = model.ProxyType,
                    ProxyTypeConfig = model.ProxyTypeConfig
                };
                modalLoading.Show(TextDuplicateProxyRule, null, true);
                try
                {
                    newModel = await GlashClient.SaveProxyRule(newModel);
                    GlashClient.LoadProxyRule(newModel);
                    _ = InvokeAsync(StateHasChanged);
                    modalWindow.Close();
                }
                catch (Exception ex)
                {
                    modalAlert.Show(TextError, ex.Message);
                }
                modalLoading.Close();
            });
        }

        private void EditProxyRule(ProxyRuleInfo model)
        {
            modalWindow.Show<Controls.EditProxyRule>(Locale.GetString(TextEditProxyRule), Controls.EditProxyRule.PrepareParameter(
                JsonConvert.DeserializeObject<ProxyRuleInfo>(JsonConvert.SerializeObject(model)),
                async editModel =>
                {
                    modalLoading.Show(Locale.GetString(TextEditProxyRule), null, true);
                    try
                    {
                        model.Name = editModel.Name;
                        model.LocalIPAddress = editModel.LocalIPAddress;
                        model.LocalPort = editModel.LocalPort;
                        model.RemoteHost = editModel.RemoteHost;
                        model.RemotePort = editModel.RemotePort;
                        model.ProxyType = editModel.ProxyType;
                        model.ProxyTypeConfig = editModel.ProxyTypeConfig;

                        GlashClient.UnloadProxyRule(model.Id);
                        model = await GlashClient.SaveProxyRule(model);
                        GlashClient.LoadProxyRule(model);
                        _ = InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(TextError, ex.Message);
                    }
                    modalLoading.Close();
                }
            ));
        }

        private void DeleteProxyRule(ProxyRuleInfo model)
        {
            modalAlert.Show(
                TextDeleteProxyRule,
                Locale.GetString("Are you sure to delete ProxyRule[{0}]?", model.Name),
                async () =>
                {
                    modalLoading.Show(TextDeleteProxyRule, null, true);
                    try
                    {
                        GlashClient.UnloadProxyRule(model.Id);
                        await GlashClient.DeleteProxyRule(model.Id);
                        _ = InvokeAsync(StateHasChanged);
                    }
                    catch (Exception ex)
                    {
                        _ = Task.Delay(100).ContinueWith(t =>
                        {
                            modalAlert.Show(TextError, ex.Message);
                        });
                    }
                    modalLoading.Close();
                });
        }

        private async Task onProxyRuleEnableChanged(ProxyRuleContext proxyRuleContext)
        {
            await Task.Delay(100);
            try
            {
                if (proxyRuleContext.Config.Enable)
                    GlashClient.EnableProxyRule(proxyRuleContext);
                else
                    GlashClient.DisableProxyRule(proxyRuleContext);
            }
            catch (Exception ex)
            {
                modalAlert.Show(TextError, ex.Message);
                await Task.Delay(100);
                await InvokeAsync(StateHasChanged);
            }
        }

        private ProxyRuleContext[] GetProxyRuleContexts(string agent)
        {
            return GlashClient.ProxyRuleContexts
                .Where(t => t.Config.Agent == agent)
                .OrderBy(t => t.Config.Name)
                .ToArray();
        }

        private void EnableAllProxyRules(string agent)
        {
            foreach (var item in GetProxyRuleContexts(agent))
                try { GlashClient.EnableProxyRule(item); }
                catch { }
            InvokeAsync(StateHasChanged);
        }

        private void DisableAllProxyRules(string agent)
        {
            foreach (var item in GetProxyRuleContexts(agent))
                try { GlashClient.DisableProxyRule(item); }
                catch { }
            InvokeAsync(StateHasChanged);
        }
    }
}
