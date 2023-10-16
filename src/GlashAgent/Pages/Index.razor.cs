using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;

namespace GlashAgent.Pages
{
    public partial class Index
    {
        public enum Texts
        {
            Title,
            LoginPasswordManage,
            ProfileManage
        }

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
                Message = "密码不正确！";
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
