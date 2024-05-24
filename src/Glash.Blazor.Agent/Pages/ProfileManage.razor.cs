using Newtonsoft.Json;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;

namespace Glash.Blazor.Agent.Pages
{
    public partial class ProfileManage : ComponentBase_WithGettextSupport
    {
        private ModalWindow modalWindow;
        private ModalAlert modalAlert;

        private static string TextName => Locale.GetString("Name");
        private static string TextAgentName => Locale.GetString("Agent Name");
        private static string TextStatus => Locale.GetString("Status");
        private static string TextOperate => Locale.GetString("Operate");
        private static string TextAdd => Locale.GetString("Add");
        private static string TextEdit => Locale.GetString("Edit");
        private static string TextDelete => Locale.GetString("Delete");

        private CancellationTokenSource cts = new CancellationTokenSource();

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
                beginRefresh(cts.Token);
            }
        }

        private void beginRefresh(CancellationToken token)
        {
            Task.Delay(1000, token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;
                InvokeAsync(StateHasChanged);
                beginRefresh(token);
            });
        }

        public override void Dispose()
        {
            base.Dispose();
            cts.Cancel();
        }

        private void Add()
        {
            modalWindow.Show<Controls.EditProfile>(Locale.GetString("Add"), Controls.EditProfile.PrepareParameter(
                new Model.Profile(Guid.NewGuid().ToString("N")),
                model =>
                {
                    try
                    {
                        ConfigDbContext.CacheContext.Add(model);
                        Core.GlashAgentManager.Instance.OnAdd(model);
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(Locale.GetString("Error"), ex.Message);
                    }
                }
            ));
        }

        private void Edit(Model.Profile model)
        {
            modalWindow.Show<Controls.EditProfile>(Locale.GetString("Edit"), Controls.EditProfile.PrepareParameter(
                JsonConvert.DeserializeObject<Model.Profile>(JsonConvert.SerializeObject(model)),
                editModel =>
                {
                    try
                    {
                        Core.GlashAgentManager.Instance.OnDelete(model);
                        model.Name = editModel.Name;
                        model.ServerUrl = editModel.ServerUrl;
                        model.AgentName = editModel.AgentName;
                        model.AgentPassword = editModel.AgentPassword;
                        ConfigDbContext.CacheContext.Update(model);
                        Core.GlashAgentManager.Instance.OnAdd(model);
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(Locale.GetString("Error"), ex.Message);
                    }
                }
            ));
        }

        private void Delete(Model.Profile model)
        {
            modalAlert.Show(Locale.GetString("Delete"), Locale.GetString("Are you sure to delete profile[{0}]?", model.Name), () =>
            {
                try
                {
                    ConfigDbContext.CacheContext.Remove(model, true);
                    Core.GlashAgentManager.Instance.OnDelete(model);
                    InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Task.Delay(100).ContinueWith(t =>
                    {
                        modalAlert.Show(Locale.GetString("Error"), ex.Message);
                    });
                }
            });
        }
    }
}
