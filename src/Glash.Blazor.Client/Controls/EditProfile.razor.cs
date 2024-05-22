using Microsoft.AspNetCore.Components;
using Quick.Localize;
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

        private static string TextName=>Locale.GetString("Name");
        private static string TextServerUrl=>Locale.GetString("Server Url");
        private static string TextClientName=>Locale.GetString("Client Name");
        private static string TextClientPassword=>Locale.GetString("Client Password");
        private static string TextOk=>Locale.GetString("OK");

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
