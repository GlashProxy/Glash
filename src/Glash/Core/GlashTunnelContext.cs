using Newtonsoft.Json.Serialization;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Core
{
    public class GlashTunnelContext : IDisposable
    {
        private CancellationTokenSource cts;
        private byte[] buffer = new byte[1024];
        private QpChannel channel;
        private TunnelInfo tunnelInfo;
        private Stream stream;
        private Action<Exception> errorHandler;

        public GlashTunnelContext(QpChannel channel, TunnelInfo tunnelInfo, Stream stream, Action<Exception> errorHandler)
        {
            this.channel = channel;
            this.tunnelInfo = tunnelInfo;
            this.stream = stream;
            this.errorHandler = errorHandler;
        }

        private async Task beginRead(CancellationToken token)
        {
            try
            {
                var task =stream?.ReadAsync(buffer, 0, buffer.Length, token);
                if (task == null)
                    return;
                var ret = await task;
                if (ret <= 0)
                    throw new IOException("Read count: " + ret);
                await channel.SendNoticePackage(new G.D()
                {
                    TunnelId = tunnelInfo.Id,
                    Data = buffer.Take(ret).ToArray()
                });
                _ = beginRead(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void PushData(byte[] data)
        {
            try
            {
                stream?.Write(data);
                stream?.Flush();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void OnError(Exception ex)
        {
            errorHandler?.Invoke(ex);
        }

        public void Start()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            _ = beginRead(cts.Token);
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts = null;
            try
            {
                stream?.Dispose();
                stream = null;
            }
            catch { }
        }
    }
}
