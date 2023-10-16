using Glash.Client;
using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;

namespace Glash.Blazor.Client
{
    public partial class Login
    {
        public enum Texts
        {
            Title,
            ChooseProfile,
            Login,
            ProfileManage
        }

        private ModalWindow modalWindow;
        private ModalAlert modalAlert;
        private ModalLoading modalLoading;

        [Parameter]
        public INavigator INavigator { get; set; }
        private string _CurrentProfileId;
        public string CurrentProfileId
        {
            get { return _CurrentProfileId; }
            set
            {
                _CurrentProfileId = value;
                Global.Instance.Profile = null;
                if (!string.IsNullOrEmpty(value))
                    Global.Instance.Profile = ConfigDbContext.CacheContext.Find(new Model.Profile(value));
            }
        }

        private void ShowProfileManageWindow()
        {
            modalWindow.Show<ProfileManage>(
                Global.Instance.TextManager.GetText(Texts.ProfileManage),
                ProfileManage.PrepareParameter(
                    () => InvokeAsync(StateHasChanged)
                )
            );
        }

        private async Task OnPost()
        {
            modalLoading.Show(null, null, true);
            try
            {
                var glashClient = new GlashClient(Global.Instance.Profile.ServerUrl);
                await glashClient.ConnectAsync(Global.Instance.Profile.ClientName, Global.Instance.Profile.ClientPassword);
                var agentList = await glashClient.GetAgentListAsync();
                agentList = agentList
                    .OrderBy(t => t.AgentName)
                    .OrderByDescending(t => t.IsLoggedIn)
                    .ToArray();
                var proxyRuleList = await glashClient.GetProxyRuleListAsync();
                proxyRuleList = proxyRuleList
                    .OrderBy(t => t.Name)
                    .ToArray();
                Global.Instance.GlashClient = glashClient;
                INavigator.Navigate<Main>(Main.PrepareParameter(Global.Instance.Profile, glashClient, agentList, proxyRuleList));
            }
            catch (Exception ex)
            {
                Global.Instance.GlashClient?.Dispose();
                Global.Instance.GlashClient = null;
                modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
            }
            modalLoading.Close();
        }
    }
}
