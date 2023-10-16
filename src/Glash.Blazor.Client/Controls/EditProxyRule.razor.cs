using Glash.Blazor.Client.ProxyTypes;
using Glash.Client.Protocol.QpModel;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.Controls
{
    public partial class EditProxyRule
    {
        [Parameter]
        public ProxyRuleInfo Model { get; set; }
        private IProxyType ProxyType;
        private RenderFragment ProxyTypeUI;

        [Parameter]
        public Action<ProxyRuleInfo> OkAction { get; set; }

        private void Ok()
        {
            if (ProxyType == null)
                Model.ProxyTypeConfig = null;
            else
                Model.ProxyTypeConfig = JsonConvert.SerializeObject(ProxyType);
            OkAction?.Invoke(Model);
        }

        public static Dictionary<string, object> PrepareParameter(ProxyRuleInfo model, Action<ProxyRuleInfo> okAction)
        {
            return new Dictionary<string, object>()
            {
                [nameof(Model)] = model,
                [nameof(OkAction)] = okAction,
            };
        }
        
        protected override void OnParametersSet()
        {
            refreshProxyType();
        }

        private void refreshProxyType()
        {
            if (string.IsNullOrEmpty(Model.ProxyType))
            {
                ProxyType = null;
                ProxyTypeUI = null;
            }
            else
            {
                ProxyType = ProxyTypeManager.Instance
                    .GetProxyTypeInfo(Model.ProxyType).CreateInstance(Model.ProxyTypeConfig);
                ProxyTypeUI = ProxyType.GetUI();
            }
        }

        private void onProxyTypeChanged(string value)
        {
            Model.ProxyType = value;
            refreshProxyType();
            InvokeAsync(StateHasChanged);
        }
    }
}
