using Glash.Client.Protocol.QpModel;
using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace Glash.Blazor.Client.Controls
{
    public partial class EditProxyRule : ComponentBase_WithGettextSupport
    {
        [Parameter]
        public ProxyRuleInfo Model { get; set; }

        [Parameter]
        public Action<ProxyRuleInfo> OkAction { get; set; }

        private static string TextName=>Locale.GetString("Name");
        private static string TextAgent=>Locale.GetString("Agent");        
        private static string TextLocal=>Locale.GetString("Local");
        private static string TextRemote=>Locale.GetString("Remote");
        private static string TextOk=>Locale.GetString("OK");

        private void Ok()
        {
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
    }
}
