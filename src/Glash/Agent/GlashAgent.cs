using Glash.Core;
using Quick.Protocol;
using Quick.Protocol.Utils;
using System.Net.Sockets;

namespace Glash.Agent
{
    public class GlashAgent : IDisposable
    {
        private QpClientOptions qpClientOptions;
        private QpClient qpClient;
        private string agentName;
        private string password;
        private Dictionary<int, GlashTunnelContext> tunnelContextDict = new Dictionary<int, GlashTunnelContext>();

        public event EventHandler Disconnected;
        public event EventHandler<string> LogPushed;

        public GlashAgent(string url, string agentName, string password)
        {
            this.password = password;
            this.agentName = agentName;

            var commandExecuterManager = new CommandExecuterManager();
            commandExecuterManager.Register(
                new Protocol.QpCommands.CreateTunnel.Request(),
                executeCommand_CreateTunnel);
            commandExecuterManager.Register(
                new Protocol.QpCommands.StartTunnel.Request(),
                executeCommand_StartTunnel);

            var noticeHandlerManager = new NoticeHandlerManager();
            noticeHandlerManager.Register<G.D>(OnTunnelDataAviliable);
            noticeHandlerManager.Register<TunnelClosed>(OnTunnelClosed);

            qpClientOptions = QpClientOptions.Parse(new Uri(url));
            qpClientOptions.InstructionSet = [Protocol.Instruction.Instance];
            qpClientOptions.RegisterCommandExecuterManager(commandExecuterManager);
            qpClientOptions.RegisterNoticeHandlerManager(noticeHandlerManager);
        }

        private void QpClient_Disconnected(object sender, EventArgs e)
        {
            LogPushed?.Invoke(this, $"Disconnected.Message:{ExceptionUtils.GetExceptionMessage(qpClient?.LastException)}");
            GlashTunnelContext[] tunnels = null;
            lock (tunnelContextDict)
            {
                tunnels = tunnelContextDict.Values.ToArray();
                tunnelContextDict.Clear();

                if (qpClient != null)
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    clean();
                }
            }
            foreach (var tunnel in tunnels)
                tunnel.Dispose();
        }

        private void clean()
        {
            var client = qpClient;
            if (client != null)
            {
                client.Disconnected -= QpClient_Disconnected;
                client.Close();
                qpClient = null;
            }
        }

        public async Task ConnectAsync()
        {
            try
            {
                clean();
                qpClient = qpClientOptions.CreateClient();
                qpClient.Disconnected += QpClient_Disconnected;
                //Connect
                await qpClient.ConnectAsync();
                //Register
                var answer = CryptoUtils.GetAnswer(qpClient.AuthenticateQuestion, password);
                await qpClient.SendCommand(new Protocol.QpCommands.Login.Request()
                {
                    Name = agentName,
                    Answer = answer
                });
            }
            catch
            {
                clean();
                throw;
            }
        }

        public void Dispose()
        {
            clean();
        }

        private Protocol.QpCommands.CreateTunnel.Response executeCommand_CreateTunnel(
            QpChannel channel,
            Protocol.QpCommands.CreateTunnel.Request request)
        {
            var tunnelInfo = request.Data;
            var tunnelId = tunnelInfo.Id;
            try
            {
                var tcpClient = new TcpClient();
                tcpClient.Connect(tunnelInfo.Host, tunnelInfo.Port);
                var tunnelContext = new GlashTunnelContext(
                    channel, tunnelInfo,
                    tcpClient.GetStream(),
                    ex =>
                    {
                        LogPushed?.Invoke(this, $"Tunnel[{tunnelId}] error.Message:{ExceptionUtils.GetExceptionMessage(ex)}");
                        channel.SendNoticePackage(new Core.TunnelClosed() { TunnelId = tunnelId });
                        GlashTunnelContext tunnelContext = null;
                        lock (tunnelContextDict)
                        {
                            if (!tunnelContextDict.ContainsKey(tunnelId))
                                return;
                            tunnelContext = tunnelContextDict[tunnelId];
                        }
                        tunnelContext.Dispose();
                    });
                lock (tunnelContextDict)
                    tunnelContextDict[tunnelId] = tunnelContext;
                LogPushed?.Invoke(this, $"Create tunnel[{tunnelId}] to {tunnelInfo.Host}:{tunnelInfo.Port} success.");
                return new Protocol.QpCommands.CreateTunnel.Response();
            }
            catch (Exception ex)
            {
                LogPushed?.Invoke(this, $"Create tunnel[{tunnelId}] to {tunnelInfo.Host}:{tunnelInfo.Port} error.Reason: {ExceptionUtils.GetExceptionMessage(ex)}");
                throw;
            }
        }

        private Protocol.QpCommands.StartTunnel.Response executeCommand_StartTunnel(
            QpChannel channel,
            Protocol.QpCommands.StartTunnel.Request request)
        {
            var tunnelId = request.TunnelId;
            GlashTunnelContext tunnelContext;
            if (!tunnelContextDict.TryGetValue(tunnelId, out tunnelContext))
                throw new ApplicationException($"Tunnel[{tunnelId}] not exist.");
            tunnelContext.Start();
            return new Protocol.QpCommands.StartTunnel.Response();
        }

        private void OnTunnelDataAviliable(QpChannel channel, G.D data)
        {
            var tunnelId = data.TunnelId;
            if (!tunnelContextDict.ContainsKey(tunnelId))
                return;
            var tunnelContext = tunnelContextDict[tunnelId];
            tunnelContext.PushData(data.Data);
        }

        private void OnTunnelClosed(QpChannel channel, TunnelClosed data)
        {
            var tunnelId = data.TunnelId;
            GlashTunnelContext tunnelContext = null;
            lock (tunnelContextDict)
            {
                if (!tunnelContextDict.ContainsKey(tunnelId))
                    return;
                tunnelContext = tunnelContextDict[tunnelId];
                tunnelContextDict.Remove(tunnelId);
            }
            tunnelContext.Dispose();
            LogPushed?.Invoke(this, $"Tunnel[{tunnelId}] closed.");
        }
    }
}
