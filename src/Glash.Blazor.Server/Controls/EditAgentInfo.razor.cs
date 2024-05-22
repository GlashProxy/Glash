using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace Glash.Blazor.Server.Controls
{
    public partial class EditAgentInfo : ComponentBase_WithGettextSupport
    {
        private bool IsAdd;
        [Parameter]
        public Model.AgentInfo Model { get; set; }

        [Parameter]
        public Action<Model.AgentInfo> OkAction { get; set; }

        private static string TextName=> Locale.GetString("Name");
        private static string TextPassword=> Locale.GetString("Password");
        private static string TextOk=> Locale.GetString("OK");
        
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            IsAdd = Model.Name == null;
        }

        private void Ok()
        {
            OkAction?.Invoke(Model);
        }

        public static Dictionary<string, object> PrepareParameter(Model.AgentInfo model, Action<Model.AgentInfo> okAction)
        {
            return new Dictionary<string, object>()
            {
                [nameof(Model)] = model,
                [nameof(OkAction)] = okAction,
            };
        }
    }
}
