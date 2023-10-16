using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client
{
    public partial class Index : INavigator
    {
        private ModalAlert modalAlert;
        private RenderFragment Body;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Navigate<Login>();
        }

        public void Navigate<TPage>(Dictionary<string, object> parameterDict = null)
        {
            if (parameterDict == null)
                parameterDict = new Dictionary<string, object>();
            parameterDict[nameof(INavigator)] = this;
            Body = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<TPage>(parameterDict);
            InvokeAsync(StateHasChanged);
        }

        public void Alert(string title, string message)
        {
            modalAlert.Show(title, message);
        }
    }
}
