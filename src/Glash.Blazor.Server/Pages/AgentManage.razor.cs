using Glash.Server;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;

namespace Glash.Blazor.Server.Pages
{
    public partial class AgentManage : IDisposable
    {
        private ModalWindow modalWindow;
        private ModalAlert modalAlert;

        [Parameter]
        public Action AgentChangedHandler { get; set; }

        private static string TextName => Locale.GetString("Name");
        private static string TextChannelName => Locale.GetString("Channel Name");
        private static string TextConnectTime => Locale.GetString("Connect Time");

        private static string TextOperate => Locale.GetString("Operate");
        private static string TextAdd => Locale.GetString("Add");
        private static string TextEdit => Locale.GetString("Edit");
        private static string TextDelete => Locale.GetString("Delete");
        private static string TextError => Locale.GetString("Error");

        private void Add()
        {
            modalWindow.Show<Controls.EditAgentInfo>(TextAdd, Controls.EditAgentInfo.PrepareParameter(
                new Model.AgentInfo(),
                model =>
                {
                    try
                    {
                        ConfigDbContext.CacheContext.Add(model);
                        AgentChangedHandler?.Invoke();
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

        private void Edit(Model.AgentInfo model)
        {
            modalWindow.Show<Controls.EditAgentInfo>(TextEdit, Controls.EditAgentInfo.PrepareParameter(
                JsonConvert.DeserializeObject<Model.AgentInfo>(JsonConvert.SerializeObject(model)),
                editModel =>
                {
                    try
                    {
                        if (model.Context != null)
                            model.Context.Dispose();
                        model.Password = editModel.Password;
                        ConfigDbContext.CacheContext.Update(model);
                        AgentChangedHandler?.Invoke();
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

        private void Delete(Model.AgentInfo model)
        {
            modalAlert.Show(TextDelete, Locale.GetString("Are you sure to delete agent[{0}]?", model.Name), () =>
            {
                try
                {
                    if (model.Context != null)
                        model.Context.Dispose();
                    ConfigDbContext.CacheContext.Remove(model, true);
                    AgentChangedHandler?.Invoke();
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
                [nameof(AgentChangedHandler)] = profileChangedHandler
            };
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Global.Instance.GlashServer.AgentConnected += GlashServer_AgentConnectedOrDisconnected;
            Global.Instance.GlashServer.AgentDisconnected += GlashServer_AgentConnectedOrDisconnected;
        }

        private void GlashServer_AgentConnectedOrDisconnected(object sender, GlashAgentContext e)
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Global.Instance.GlashServer.AgentConnected -= GlashServer_AgentConnectedOrDisconnected;
            Global.Instance.GlashServer.AgentDisconnected -= GlashServer_AgentConnectedOrDisconnected;
        }
    }
}
