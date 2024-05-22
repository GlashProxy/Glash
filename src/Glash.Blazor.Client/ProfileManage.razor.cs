using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;
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

        [Parameter]
        public Action ProfileChangedHandler { get; set; }

        private static string TextName => Locale.GetString("Name");
        private static string TextClientName => Locale.GetString("Client Name");
        private static string TextOperate => Locale.GetString("Operate");
        private static string TextAdd => Locale.GetString("Add");
        private static string TextEdit => Locale.GetString("Edit");
        private static string TextDelete => Locale.GetString("Delete");
        private static string TextError => Locale.GetString("Error");


        private void Add()
        {
            modalWindow.Show<Controls.EditProfile>(TextAdd, Controls.EditProfile.PrepareParameter(
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
                        modalAlert.Show(TextError, ex.Message);
                    }
                }
            ));
        }

        private void Edit(Model.Profile model)
        {
            modalWindow.Show<Controls.EditProfile>(TextEdit, Controls.EditProfile.PrepareParameter(
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
                        modalAlert.Show(TextError, ex.Message);
                    }
                }
            ));
        }

        private void Delete(Model.Profile model)
        {
            modalAlert.Show(TextDelete, Locale.GetString("Are you sure to delete Profile[{0}]?", model.Name), () =>
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
                        modalAlert.Show(TextError, ex.Message);
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
