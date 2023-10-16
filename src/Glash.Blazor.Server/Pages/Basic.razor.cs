using Quick.Blazor.Bootstrap;
using System.ComponentModel.DataAnnotations;

namespace Glash.Blazor.Server.Pages
{
    public partial class Basic
    {
        [Required]
        private string ConnectionPassword { get; set; }
        private ModalAlert modalAlert;

        public enum Texts
        {
            ConnectionPassword,
            ApiAddress,
            ChangeConnectionPasswordSuccess
        }

        protected override void OnInitialized()
        {
            ConnectionPassword = Global.Instance.ConnectionPassword;
        }

        private string getGlashServerUrl()
        {
            var httpUrl = NavigationManager.Uri;
            var httpUri = new Uri(httpUrl);
            string scheme;
            string hostAndPort = null;
            switch (httpUri.Scheme)
            {
                case "http":
                    scheme = "qp.ws";
                    if (httpUri.Port == 80)
                        hostAndPort = httpUri.Host;
                    break;
                case "https":
                    scheme = "qp.wss";
                    if (httpUri.Port == 443)
                        hostAndPort = httpUri.Host;
                    break;
                default:
                    throw new ArgumentException($"Unknown schema: {httpUri.Scheme}");
            }
            if (string.IsNullOrEmpty(hostAndPort))
                hostAndPort = $"{httpUri.Host}:{httpUri.Port}";
            return $"{scheme}://{hostAndPort}{httpUri.PathAndQuery}glash?Password={Global.Instance.ConnectionPassword}";
        }

        private void Ok()
        {
            try
            {
                Global.Instance.ConnectionPassword = ConnectionPassword;
                modalAlert.Show(
                    Global.Instance.TextManager.GetText(ServerTexts.Success),
                    Global.Instance.TextManager.GetText(Texts.ChangeConnectionPasswordSuccess));
            }
            catch (Exception ex)
            {
                modalAlert.Show(
                    Global.Instance.TextManager.GetText(ServerTexts.Error),
                    ex.Message);
            }
        }
    }
}
