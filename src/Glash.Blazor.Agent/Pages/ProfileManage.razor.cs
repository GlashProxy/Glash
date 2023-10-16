using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;

namespace Glash.Blazor.Agent.Pages
{
    public partial class ProfileManage : IDisposable
    {
        private ModalWindow modalWindow;
        private ModalAlert modalAlert;

        public enum Texts
        {
            Operate,
            Add,
            Edit,
            Delete,
            DeleteConfirm
        }

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

        public void Dispose()
        {
            cts.Cancel();
        }

        private void Add()
        {
            modalWindow.Show<Controls.EditProfile>(Global.Instance.TextManager.GetText(Texts.Add), Controls.EditProfile.PrepareParameter(
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
                        modalAlert.Show(Global.Instance.TextManager.GetText(AgentTexts.Error), ex.Message);
                    }
                }
            ));
        }

        private void Edit(Model.Profile model)
        {
            modalWindow.Show<Controls.EditProfile>(Global.Instance.TextManager.GetText(Texts.Edit), Controls.EditProfile.PrepareParameter(
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
                        modalAlert.Show(Global.Instance.TextManager.GetText(AgentTexts.Error), ex.Message);
                    }
                }
            ));
        }

        private void Delete(Model.Profile model)
        {
            modalAlert.Show(Global.Instance.TextManager.GetText(Texts.Delete), Global.Instance.TextManager.GetText(Texts.DeleteConfirm, model.Name), () =>
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
                        modalAlert.Show(Global.Instance.TextManager.GetText(AgentTexts.Error), ex.Message);
                    });
                }
            });
        }
    }
}
