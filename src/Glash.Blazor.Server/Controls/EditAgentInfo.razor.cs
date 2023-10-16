using Microsoft.AspNetCore.Components;

namespace Glash.Blazor.Server.Controls
{
    public partial class EditAgentInfo
    {
        private bool IsAdd;
        [Parameter]
        public Model.AgentInfo Model { get; set; }

        [Parameter]
        public Action<Model.AgentInfo> OkAction { get; set; }

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
