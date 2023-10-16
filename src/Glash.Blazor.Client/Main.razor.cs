using Glash.Blazor.Client.ProxyTypes;
using Glash.Client;
using Glash.Client.Protocol.QpModel;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin;
using Quick.Blazor.Bootstrap.Admin.Utils;

namespace Glash.Blazor.Client
{
    public partial class Main
    {
        public enum Texts
        {
            AddProxyRule,
            DuplicateProxyRule,
            EditProxyRule,
            DeleteProxyRule,
            DeleteProxyRuleConfirm,
            Logout,
            LogoutConfirm,
            EnableAll,
            DisableAll,
            System,
            Local,
            Remote,
            DisplayRows,
            DisconnectedFromServer,
            AgentNotLogin
        }


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
                Global.Instance.TextManager.GetText(ClientTexts.Error),
                Global.Instance.TextManager.GetText(Texts.DisconnectedFromServer));
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
                @Global.Instance.TextManager.GetText(Texts.Logout),
                @Global.Instance.TextManager.GetText(Texts.LogoutConfirm),
                () =>
                {
                    logout();
                }
            );
        }

        private void AddProxyRule(string agent)
        {
            modalWindow.Show<Controls.EditProxyRule>(Global.Instance.TextManager.GetText(Texts.AddProxyRule), Controls.EditProxyRule.PrepareParameter(
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
                    modalLoading.Show(Global.Instance.TextManager.GetText(Texts.AddProxyRule), null, true);
                    try
                    {
                        model = await GlashClient.SaveProxyRule(model);
                        GlashClient.LoadProxyRule(model);
                        _ = InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
                    }
                    modalLoading.Close();
                }
            ));
        }

        private void DuplicateProxyRule(ProxyRuleInfo model)
        {
            modalPrompt.Show(@Global.Instance.TextManager.GetText(Texts.DuplicateProxyRule), model.Name, async newName =>
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
                modalLoading.Show(Global.Instance.TextManager.GetText(Texts.DuplicateProxyRule), null, true);
                try
                {
                    newModel = await GlashClient.SaveProxyRule(newModel);
                    GlashClient.LoadProxyRule(newModel);
                    _ = InvokeAsync(StateHasChanged);
                    modalWindow.Close();
                }
                catch (Exception ex)
                {
                    modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
                }
                modalLoading.Close();
            });
        }

        private void EditProxyRule(ProxyRuleInfo model)
        {
            modalWindow.Show<Controls.EditProxyRule>(Global.Instance.TextManager.GetText(Texts.EditProxyRule), Controls.EditProxyRule.PrepareParameter(
                JsonConvert.DeserializeObject<ProxyRuleInfo>(JsonConvert.SerializeObject(model)),
                async editModel =>
                {
                    modalLoading.Show(Global.Instance.TextManager.GetText(Texts.EditProxyRule), null, true);
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
                        modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
                    }
                    modalLoading.Close();
                }
            ));
        }

        private void DeleteProxyRule(ProxyRuleInfo model)
        {
            modalAlert.Show(
                Global.Instance.TextManager.GetText(Texts.DeleteProxyRule),
                Global.Instance.TextManager.GetText(Texts.DeleteProxyRuleConfirm, model.Name),
                async () =>
                {
                    modalLoading.Show(Global.Instance.TextManager.GetText(Texts.DeleteProxyRule), null, true);
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
                            modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
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
                modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
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

        private IProxyType GetProxyTypeInstance(ProxyRuleContext proxyRuleContext)
        {
            var proxyType = proxyRuleContext.GetExtendProperty<IProxyType>();
            if (proxyType == null)
            {
                if (string.IsNullOrEmpty(proxyRuleContext.Config.ProxyType))
                    return null;
                proxyType = ProxyTypeManager.Instance
                    .GetProxyTypeInfo(proxyRuleContext.Config.ProxyType)
                    .CreateInstance(proxyRuleContext.Config.ProxyTypeConfig);
                proxyRuleContext.SetExtendProperty(proxyType);
            }
            return proxyType;
        }

        private string GetProxyRuleContextIcon(ProxyRuleContext proxyRuleContext)
        {
            return GetProxyTypeInstance(proxyRuleContext)?.Icon ?? "fa fa-cube";
        }

        private async Task ButtonInvoke(ProxyTypeButton button, ProxyRuleContext proxyRuleContext)
        {
            try
            {
                modalLoading.Show(button.Name, null, true);
                await Task.Run(() => button.Invoke(proxyRuleContext));
            }
            catch (Exception ex)
            {
                modalAlert.Show(button.Name, ExceptionUtils.GetExceptionString(ex));
            }
            finally
            {
                modalLoading.Close();
            }
        }
    }
}
