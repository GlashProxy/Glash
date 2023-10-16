using Glash.Core;
using Quick.Protocol;

namespace Glash.Server
{
    public class GlashServerTunnelContext : IDisposable
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        public TunnelInfo TunnelInfo { get; private set; }
        public GlashClientContext Client { get; private set; }
        public GlashAgentContext Agent { get; private set; }
        public DateTime CreateTime { get; private set; }
        public long UploadBytes { get; private set; }
        public long DownloadBytes { get; private set; }
        public long UploadBytesPerSecond { get; private set; }
        public long DownloadBytesPerSecond { get; private set; }
        private long preUploadBytes, preDownloadBytes;
        private Action<Exception> errorHandler;

        public GlashServerTunnelContext(
            TunnelInfo tunnelInfo,
            GlashClientContext client,
            GlashAgentContext agent,
            Action<Exception> errorHandler)
        {
            this.TunnelInfo = tunnelInfo;
            this.Client = client;
            this.Agent = agent;
            this.errorHandler = errorHandler;
            CreateTime = DateTime.Now;
            cts = new CancellationTokenSource();
            beginCalcSpeed(cts.Token);
        }

        private void beginCalcSpeed(CancellationToken cancellationToken)
        {
            Task.Delay(1000, cancellationToken).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;
                try
                {
                    var currentUploadBytes = UploadBytes;
                    var currentDownloadBytes = DownloadBytes;

                    UploadBytesPerSecond = currentUploadBytes - preUploadBytes;
                    if (UploadBytesPerSecond < 0)
                        UploadBytesPerSecond = 0;
                    preUploadBytes = currentUploadBytes;

                    DownloadBytesPerSecond = currentDownloadBytes - preDownloadBytes;
                    if (DownloadBytesPerSecond < 0)
                        DownloadBytesPerSecond = 0;
                    preDownloadBytes = currentDownloadBytes;
                }
                catch { }
                beginCalcSpeed(cancellationToken);
            });
        }

        public void OnError(Exception ex)
        {
            errorHandler?.Invoke(ex);
        }

        private void PushData(QpChannel channel, byte[] data)
        {
            try
            {
                channel.SendNoticePackage(new G.D()
                {
                    TunnelId = TunnelInfo.Id,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void PushDataToClient(byte[] data)
        {
            PushData(Client.Channel, data);
            DownloadBytes += data.Length;
        }

        public void PushDataToAgent(byte[] data)
        {
            PushData(Agent.Channel, data);
            UploadBytes += data.Length;
        }

        public void SendTunnelClosedNotice(QpChannel channel)
        {
            channel.SendNoticePackage(new TunnelClosed() { TunnelId = TunnelInfo.Id });
        }

        public void SendTunnelClosedNoticeToClient()
        {
            SendTunnelClosedNotice(Client.Channel);
        }

        public void SendTunnelClosedNoticeToAgent()
        {
            SendTunnelClosedNotice(Agent.Channel);
        }

        public void StartAgentTunnel()
        {
            Agent.StartTunnelAsync(TunnelInfo.Id).Wait();
        }

        public void Dispose()
        {
            cts.Cancel();
        }
    }
}
