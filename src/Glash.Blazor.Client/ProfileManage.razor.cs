using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.EntityFrameworkCore.Plus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client
{
    public partial class ProfileManage
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

        [Parameter]
        public Action ProfileChangedHandler { get; set; }

        private void Add()
        {
            modalWindow.Show<Controls.EditProfile>(Global.Instance.TextManager.GetText(Texts.Add), Controls.EditProfile.PrepareParameter(
                new Model.Profile(Guid.NewGuid().ToString("N")),
                model =>
                {
                    try
                    {
                        ConfigDbContext.CacheContext.Add(model);
                        ProfileChangedHandler?.Invoke();
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
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
                        model.Name = editModel.Name;
                        model.ServerUrl = editModel.ServerUrl;
                        model.ClientName = editModel.ClientName;
                        model.ClientPassword = editModel.ClientPassword;
                        ConfigDbContext.CacheContext.Update(model);
                        ProfileChangedHandler?.Invoke();
                        InvokeAsync(StateHasChanged);
                        modalWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
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
                    ProfileChangedHandler?.Invoke();
                    InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Task.Delay(100).ContinueWith(t =>
                    {
                        modalAlert.Show(Global.Instance.TextManager.GetText(ClientTexts.Error), ex.Message);
                    });
                }
            });
        }

        public static Dictionary<string, object> PrepareParameter(Action profileChangedHandler)
        {
            return new Dictionary<string, object>()
            {
                [nameof(ProfileChangedHandler)] = profileChangedHandler
            };
        }
    }
}
