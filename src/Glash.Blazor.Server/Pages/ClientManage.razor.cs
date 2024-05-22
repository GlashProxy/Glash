using Glash.Server;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;

namespace Glash.Blazor.Server.Pages
{
    public partial class ClientManage : IDisposable
    {
        private ModalWindow modalWindow;
        private ModalAlert modalAlert;

        [Parameter]
        public Action ClientChangedHandler { get; set; }

        private static string TextName => Locale.GetString("Name");
        private static string TextChannelName => Locale.GetString("Channel Name");
        private static string TextConnectTime => Locale.GetString("Connect Time");

        private static string TextOperate => Locale.GetString("Operate");
        private static string TextAdd => Locale.GetString("Add");
        private static string TextEdit => Locale.GetString("Edit");
        private static string TextDelete => Locale.GetString("Delete");
        private static string TextError => Locale.GetString("Error");


        private void setClientRelateAgents(string clientId, string[] agents)
        {
            var oldModels = ConfigDbContext.CacheContext
                .Query<Model.ClientAgentRelation>(t => t.ClientName == clientId)
                .ToHashSet();
            var newModels = agents.Select(t => new Model.ClientAgentRelation()
            {
                AgentName = t,
                ClientName = clientId
            }).ToHashSet();
            var toAddList = new List<Model.ClientAgentRelation>();
            var toDelList = new List<Model.ClientAgentRelation>();

            foreach (var model in newModels)
            {
                if (oldModels.Contains(model))
                    continue;
                toAddList.Add(model);
            }
            foreach (var model in oldModels)
            {
                if (newModels.Contains(model))
                    continue;
                toDelList.Add(model);
            }
            ConfigDbContext.CacheContext.AddRange(toAddList);
            ConfigDbContext.CacheContext.RemoveRange(toDelList.ToArray());
        }

        private void Add()
        {
            modalWindow.Show<Controls.EditClientInfo>(TextAdd, Controls.EditClientInfo.PrepareParameter(
                new Model.ClientInfo(),
                (model, agents) =>
                {
                    try
                    {
                        ConfigDbContext.CacheContext.Add(model);
                        setClientRelateAgents(model.Name, agents);
                        ClientChangedHandler?.Invoke();
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(TextError, ex.Message);
                    }
                }
            ));
        }

        private void Edit(Model.ClientInfo model)
        {
            modalWindow.Show<Controls.EditClientInfo>(TextEdit, Controls.EditClientInfo.PrepareParameter(
                JsonConvert.DeserializeObject<Model.ClientInfo>(JsonConvert.SerializeObject(model)),
                (editModel, agents) =>
                {
                    try
                    {
                        if (model.Context != null)
                            model.Context.Dispose();
                        model.Password = editModel.Password;
                        ConfigDbContext.CacheContext.Update(model);
                        setClientRelateAgents(model.Name, agents);
                        ClientChangedHandler?.Invoke();
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(TextError, ex.Message);
                    }
                }
            ));
        }

        private void Delete(Model.ClientInfo model)
        {
            modalAlert.Show(TextDelete, Locale.GetString("Are you sure to delete Client[{0}]?", model.Name), () =>
            {
                try
                {
                    if (model.Context != null)
                        model.Context.Dispose();
                    ConfigDbContext.CacheContext.Remove(model, true);
                    ClientChangedHandler?.Invoke();
                    InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Task.Delay(100).ContinueWith(t =>
                    {
                        modalAlert.Show(TextError, ex.Message);
                    });
                }
            });
        }

        public static Dictionary<string, object> PrepareParameter(Action profileChangedHandler)
        {
            return new Dictionary<string, object>()
            {
                [nameof(ClientChangedHandler)] = profileChangedHandler
            };
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Global.Instance.GlashServer.ClientConnected += GlashServer_ClientConnectedOrDisconnected;
            Global.Instance.GlashServer.ClientDisconnected += GlashServer_ClientConnectedOrDisconnected;
        }

        private void GlashServer_ClientConnectedOrDisconnected(object sender, GlashClientContext e)
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Global.Instance.GlashServer.ClientConnected -= GlashServer_ClientConnectedOrDisconnected;
            Global.Instance.GlashServer.ClientDisconnected -= GlashServer_ClientConnectedOrDisconnected;
        }
    }
}
