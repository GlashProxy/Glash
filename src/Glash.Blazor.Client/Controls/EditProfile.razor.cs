using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.Controls
{
    public partial class EditProfile
    {
        [Parameter]
        public Model.Profile Model { get; set; }
        [Parameter]
        public Action<Model.Profile> OkAction { get; set; }

        private void Ok()
        {
            OkAction?.Invoke(Model);
        }

        public static Dictionary<string, object> PrepareParameter(Model.Profile model, Action<Model.Profile> okAction)
        {
            return new Dictionary<string, object>()
            {
                [nameof(Model)] = model,
                [nameof(OkAction)] = okAction,
            };
        }
    }
}
