using Glash.Server;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Admin.Utils;

namespace Glash.Blazor.Server.Pages
{
    public partial class TunnelManage : IDisposable
    {
        public enum Texts
        {
            Id,
            Agent,
            Client,
            Target,
            ConnectTime,
            TotalBytes,
            CurrentSpeed,
            Operate,
            CloseTunnel,
            CloseTunnelConfirm
        }

        private ModalAlert modalAlert;
        private UnitStringConverting storageUSC = UnitStringConverting.StorageUnitStringConverting;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private GlashServerTunnelContext[] GetTunnels() => Global.Instance.GlashServer.Tunnels;
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
                Global.Instance.TextManager.GetText(Texts.CloseTunnel),
                Global.Instance.TextManager.GetText(Texts.CloseTunnelConfirm, tunnel.TunnelInfo.Id),
                () =>
                {
                    tunnel.OnError(new ApplicationException("Closed by server."));
                });
        }
    }
}
