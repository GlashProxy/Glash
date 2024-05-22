using Glash.Client;
using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;

namespace Glash.Blazor.Client
{
    public partial class Login : ComponentBase_WithGettextSupport
    {
        private static string TextTitle => Locale.GetString("Glash Client");
        private static string TextChooseProfile => Locale.GetString("Choose Profile");
        private static string TextLogin => Locale.GetString("Login");
        private static string TextProfileManage => Locale.GetString("Profile Manage");
        private static string TextError => Locale.GetString("Error");

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
                TextProfileManage,
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
                modalAlert.Show(TextError, ex.Message);
            }
            modalLoading.Close();
        }
    }
}
