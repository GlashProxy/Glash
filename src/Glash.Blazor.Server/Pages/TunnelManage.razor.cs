using Glash.Server;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin.Utils;
using Quick.Localize;

namespace Glash.Blazor.Server.Pages
{
    public partial class TunnelManage : IDisposable
    {
        private ModalAlert modalAlert;
        private UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private GlashServerTunnelContext[] GetTunnels() => Global.Instance.GlashServer.Tunnels;

        private static string TextId => Locale.GetString("Id");
        private static string TextAgent => Locale.GetString("Agent");
        private static string TextClient => Locale.GetString("Client");
        private static string TextTarget => Locale.GetString("Target");
        private static string TextConnectTime => Locale.GetString("Connect Time");
        private static string TextTotalBytes => Locale.GetString("Total Bytes");
        private static string TextCurrentSpeed => Locale.GetString("Current Speed");
        private static string TextOperate => Locale.GetString("Operate");
        private static string TextCloseTunnel => Locale.GetString("Close Tunnel");

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

        private void CloseTunnel(GlashServerTunnelContext tunnel)
        {
            modalAlert.Show(
                TextCloseTunnel,
                Locale.GetString("Are you sure close tunnel[{0}]", tunnel.TunnelInfo.Id),
                () =>
                {
                    tunnel.OnError(new ApplicationException(Locale.GetString("Closed by server.")));
                });
        }
    }
}
