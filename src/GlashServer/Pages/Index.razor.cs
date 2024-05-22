using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace GlashServer.Pages
{
    public partial class Index : ComponentBase_WithGettextSupport
    {
        private static string TextTitle => Locale.GetString("Glash Server");
        private static string TextBasic => Locale.GetString("Basic");
        private static string TextAgentManage => Locale.GetString("Agent Manage");
        private static string TextClientManage => Locale.GetString("Client Manage");
        private static string TextTunnelManage => Locale.GetString("Tunnel Manage");
        private static string TextLoginPasswordManage => Locale.GetString("Login Password Manage");
        private static string TextPleaseInputPassword => Locale.GetString("Please input password");
        private static string TextLogin => Locale.GetString("Login");
        public bool IsLogin { get; private set; } = false;
        public string Message { get; private set; }
        private string CorrectPassword => Core.LoginPasswordManager.Instance.LoginPassword;

        [BindProperty]
        public string Password { get; set; }

        [Parameter]
        public RenderFragment Body { get; set; }

        private void Show<T>()
        {
            Body = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<T>();
        }

#if DEBUG
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Password = CorrectPassword;
        }
#endif

        public void OnPost()
        {
            if (!IsLogin && CorrectPassword != Password)
            {
                Message = Locale.GetString("Password is wrong.");
                return;
            }
            IsLogin = true;
            StateHasChanged();
        }

        private void onPasswordKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                OnPost();
        }
    }
}
