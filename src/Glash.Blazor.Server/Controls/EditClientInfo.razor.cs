﻿using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.LiteDB.Plus;
using Quick.Localize;

namespace Glash.Blazor.Server.Controls
{
    public partial class EditClientInfo : ComponentBase_WithGettextSupport
    {
        private bool IsAdd;

        [Parameter]
        public Model.ClientInfo Model { get; set; }

        [Parameter]
        public Action<Model.ClientInfo, string[]> OkAction { get; set; }

        private static string TextName=> Locale.GetString("Name");
        private static string TextPassword=> Locale.GetString("Password");
        private static string TextAgent=> Locale.GetString("Agent");
        private static string TextOk=> Locale.GetString("OK");

        public class SelectAgentInfo
        {
            public string Name { get; set; }
            public bool Checked { get; set; }
        }

        private SelectAgentInfo[] selectAgents;

        private void Ok()
        {
            var agents = selectAgents
                .Where(t => t.Checked)
                .Select(t => t.Name)
                .ToArray();
            OkAction?.Invoke(Model, agents);
        }

        public static Dictionary<string, object> PrepareParameter(Model.ClientInfo model, Action<Model.ClientInfo, string[]> okAction)
        {
            return new Dictionary<string, object>()
            {
                [nameof(Model)] = model,
                [nameof(OkAction)] = okAction,
            };
        }

        private void checkAgent(SelectAgentInfo tag)
        {
            tag.Checked = !tag.Checked;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            IsAdd = Model.Name == null;
            var checkedAgentHashSet = ConfigDbContext.CacheContext
                .Query<Model.ClientAgentRelation>(t => t.ClientName == Model.Name)
                .Select(t => t.AgentName)
                .ToHashSet();

            selectAgents = ConfigDbContext.CacheContext.Query<Server.Model.AgentInfo>()
                .Select(t => new SelectAgentInfo()
                {
                    Name = t.Name,
                    Checked = checkedAgentHashSet.Contains(t.Name)
                }).ToArray();
        }
    }
}
