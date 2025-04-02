using System.Text.Json;
using Glash.Client;
using Glash.Client.Protocol.QpModel;
using Quick.Blazor.Bootstrap;
using Quick.Localize;
using Quick.Protocol.Utils;

namespace Glash.Blazor.Client
{
    public partial class Main : ComponentBase_WithGettextSupport
    {
        private static string TextProfile => Locale.GetString("Profile");
        private static string TextEnable => Locale.GetString("Enable");
        private static string TextDisable => Locale.GetString("Disable");
        private static string TextAdd => Locale.GetString("Add");
        private static string TextLogs => Locale.GetString("Logs");
        private static string TextEdit => Locale.GetString("Edit");
        private static string TextDuplicate => Locale.GetString("Duplicate");
        private static string TextDelete => Locale.GetString("Delete");
        private static string TextAgent => Locale.GetString("Agent");
        private static string TextProxyRule => Locale.GetString("Proxy Rule");
        private static string TextError => Locale.GetString("Error");
        private static string TextAddProxyRule => Locale.GetString("Add Proxy Rule");
        private static string TextDuplicateProxyRule => Locale.GetString("Duplicate Proxy Rule");
        private static string TextEditProxyRule => Locale.GetString("Edit Proxy Rule");
        private static string TextDeleteProxyRule => Locale.GetString("Delete Proxy Rule");
        private static string TextEnableAll => Locale.GetString("Enable All");
        private static string TextDisableAll => Locale.GetString("Disable All");
        private static string TextLocal => Locale.GetString("Local");
        private static string TextRemote => Locale.GetString("Remote");
        private static string TextDisplayRows => Locale.GetString("Display Rows");
        private static string TextDisconnectedFromServer => Locale.GetString("Disconnected from server");
        private static string TextAgentNotLogin => Locale.GetString("Agent not login");

        private ModalAlert modalAlert;
        private ModalWindow modalWindow;
        private ModalLoading modalLoading;
        private ModalPrompt modalPrompt;
        private ProfileContext[] ProfileContexts => ProfileContextManager.Instance.GetProfileContexts();
        //当前配置上下文
        private ProfileContext CurrentProfileContext;
        private AgentInfo CurrentAgent;

        private string _CurrentProfileId;
        public string CurrentProfileId
        {
            get { return _CurrentProfileId; }
            set
            {
                _CurrentProfileId = value;
                CurrentProfileContext = null;
                if (!string.IsNullOrEmpty(value))
                    CurrentProfileContext = ProfileContextManager.Instance.Get(value);
                CurrentAgentName = CurrentProfileContext?.Agents?.FirstOrDefault()?.AgentName;
            }
        }

        private string _CurrentAgentName;
        private string CurrentAgentName
        {
            get { return _CurrentAgentName; }
            set
            {
                _CurrentAgentName = value;
                CurrentAgent = CurrentProfileContext?.Agents?.FirstOrDefault(t => t.AgentName == value);
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            CurrentProfileId = ProfileContextManager.Instance.GetProfileContexts()?.FirstOrDefault()?.Profile?.Id;
        }

        private void AddProfile()
        {
            modalWindow.Show<Controls.EditProfile>(TextAdd, Controls.EditProfile.PrepareParameter(
                new Model.Profile(Guid.NewGuid().ToString("N")),
                model =>
                {
                    try
                    {
                        ProfileContextManager.Instance.Add(model);
                        CurrentProfileId = model.Id;
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(TextError, ex.Message);
                    }
                }
            ));
        }

        private async Task DisableProfile()
        {
            var profileContext = CurrentProfileContext;
            modalLoading.Show(TextDisable, Locale.GetString("Disabling profile[{0}]...", profileContext.Profile.Name), true);
            try
            {
                await profileContext.Disable();
            }
            catch (Exception ex)
            {
                modalAlert.Show(TextDisable, Locale.GetString("Disable profile[{0}] error.Reason: {1}", profileContext.Profile.Name, ExceptionUtils.GetExceptionMessage(ex)));
            }
            finally
            {
                modalLoading.Close();
            }
        }

        private async Task EnableProfile()
        {
            var profileContext = CurrentProfileContext;
            modalLoading.Show(TextEnable, Locale.GetString("Enabling profile[{0}]...", profileContext.Profile.Name), true);
            try
            {
                await profileContext.Enable();
                CurrentAgentName = profileContext?.Agents?.FirstOrDefault()?.AgentName;
                profileContext.EnableStateChanged += profileContext_EnableStateChanged;
                profileContext.AgentLoginStatusChanged += profileContext_AgentLoginStatusChanged;
            }
            catch (Exception ex)
            {
                modalAlert.Show(TextEnable, Locale.GetString("Enable profile[{0}] error.Reason: {1}", profileContext.Profile.Name, ExceptionUtils.GetExceptionMessage(ex)));
            }
            finally
            {
                modalLoading.Close();
            }
        }

        private void profileContext_EnableStateChanged(object sender, bool e)
        {
            var profileContext = (ProfileContext)sender;
            profileContext.EnableStateChanged -= profileContext_EnableStateChanged;
            profileContext.AgentLoginStatusChanged -= profileContext_AgentLoginStatusChanged;

            if (profileContext == CurrentProfileContext)
            {
                CurrentAgentName = null;
                InvokeAsync(StateHasChanged);
            }
        }

        private void profileContext_AgentLoginStatusChanged(object sender, AgentInfo agent)
        {
            var profileContext = (ProfileContext)sender;
            if (profileContext == CurrentProfileContext)
                InvokeAsync(StateHasChanged);
        }

        private void ShowLogs()
        {
            var model = CurrentProfileContext.Profile;
            var context = ProfileContextManager.Instance.GetContext(model);
            if (context == null)
            {
                modalAlert.Show("错误", $"未找到{model}的上下文！");
                return;
            }
            modalAlert.Show("日志", string.Join(Environment.NewLine, context.Logs), usePreTag: true);
        }

        private void EditProfile()
        {
            var model = CurrentProfileContext.Profile;
            modalWindow.Show<Controls.EditProfile>(TextEdit, Controls.EditProfile.PrepareParameter(
                JsonSerializer.Deserialize<Model.Profile>(JsonSerializer.Serialize(model)),
                editModel =>
                {
                    try
                    {
                        model.Name = editModel.Name;
                        model.ServerUrl = editModel.ServerUrl;
                        model.ClientName = editModel.ClientName;
                        model.ClientPassword = editModel.ClientPassword;
                        ProfileContextManager.Instance.Update(model);
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(TextError, ex.Message);
                    }
                }
            ));
        }

        private void DeleteProfile()
        {
            var model = CurrentProfileContext.Profile;
            modalAlert.Show(TextDelete, Locale.GetString("Are you sure to delete Profile[{0}]?", model.Name), () =>
            {
                try
                {
                    ProfileContextManager.Instance.Remove(model);
                    InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Task.Delay(100).ContinueWith(t =>
                    {
                        modalAlert.Show(TextError, ex.Message);
                    });
                }
            });
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
                        await CurrentProfileContext.AddProxyRule(model);
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
                    await CurrentProfileContext.DuplicateProxyRule(newModel);
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
                JsonSerializer.Deserialize<ProxyRuleInfo>(JsonSerializer.Serialize(model)),
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

                        await CurrentProfileContext.EditProxyRule(model);
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
                        await CurrentProfileContext.DeleteProxyRule(model);
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
                proxyRuleContext.Config.Enable = !proxyRuleContext.Config.Enable;
                if (proxyRuleContext.Config.Enable)
                    CurrentProfileContext.GlashClient.DisableProxyRule(proxyRuleContext);
                else
                    CurrentProfileContext.GlashClient.EnableProxyRule(proxyRuleContext);
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
            var ret = CurrentProfileContext?.GlashClient?.ProxyRuleContexts?
                .Where(t => t.Config.Agent == agent)?
                .OrderBy(t => t.Config.Name)?
                .ToArray();
            if (ret == null)
                ret = new ProxyRuleContext[0];
            return ret;
        }

        private void EnableAllProxyRules(string agent)
        {
            foreach (var item in GetProxyRuleContexts(agent))
                try { CurrentProfileContext.GlashClient.EnableProxyRule(item); }
                catch { }
            InvokeAsync(StateHasChanged);
        }

        private void DisableAllProxyRules(string agent)
        {
            foreach (var item in GetProxyRuleContexts(agent))
                try { CurrentProfileContext.GlashClient.DisableProxyRule(item); }
                catch { }
            InvokeAsync(StateHasChanged);
        }

        public override void Dispose()
        {
            foreach (var profileContext in ProfileContextManager.Instance.GetProfileContexts())
            {
                profileContext.EnableStateChanged -= profileContext_EnableStateChanged;
                profileContext.AgentLoginStatusChanged -= profileContext_AgentLoginStatusChanged;
            }
            CurrentAgentName = null;
            CurrentProfileId = null;
            base.Dispose();
        }
    }
}
