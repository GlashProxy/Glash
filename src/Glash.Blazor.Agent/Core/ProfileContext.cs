using Glash.Agent;
using Quick.Blazor.Bootstrap.Utils;
using Quick.Localize;

namespace Glash.Blazor.Agent.Core
{
    public class ProfileContext : IDisposable
    {
        public static int MaxLogLines = 100;
        private CancellationTokenSource cts;
        private GlashAgent glashAgent;
        private Queue<string> logQueue;
        public Model.Profile Model { get; private set; }
        public string Status { get; private set; }
        public string[] Logs
        {
            get
            {
                lock (logQueue)
                    return logQueue.ToArray();
            }
        }

        private void pushLog(string line)
        {
            line = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {line}";
            lock (logQueue)
            {
                logQueue.Enqueue(line);
                while (true)
                {
                    var currentCount = logQueue.Count;
                    if (currentCount == 0 || currentCount <= MaxLogLines)
                        break;
                    logQueue.Dequeue();
                }
            }
        }

        public ProfileContext(Model.Profile model)
        {
            Model = model;
            cts = new CancellationTokenSource();
            logQueue = new();
            glashAgent = new GlashAgent(Model.ServerUrl, Model.AgentName, Model.AgentPassword);
            glashAgent.LogPushed += GlashAgent_LogPushed;
            glashAgent.Disconnected += GlashAgent_Disconnected;
            _ = beginConnect(cts.Token);
        }

        private void GlashAgent_LogPushed(object sender, string e)
        {
            pushLog(e);
        }

        private void GlashAgent_Disconnected(object sender, EventArgs e)
        {
            Status = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {Locale.GetString("Disconnected")}";
            pushLog(Locale.GetString("Disconnected"));
            var currentCts = cts;
            if (currentCts == null)
                return;
            _ = delayToConnect(currentCts.Token);
        }

        private async Task delayToConnect(CancellationToken token)
        {
            try
            {
                await Task.Delay(5000, token);
                _ = beginConnect(token);
            }
            catch { }
        }

        private async Task beginConnect(CancellationToken token)
        {
            try
            {
                await glashAgent.ConnectAsync();
                Status = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {Locale.GetString("Connected")}";
                pushLog(Locale.GetString("Connected"));
            }
            catch (Exception ex)
            {
                pushLog(ExceptionUtils.GetExceptionMessage(ex));
                Status = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {ExceptionUtils.GetExceptionMessage(ex)}";
                _ = delayToConnect(token);
                return;
            }
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts = null;
            if (glashAgent != null)
            {
                glashAgent.LogPushed -= GlashAgent_LogPushed;
                glashAgent.Disconnected -= GlashAgent_Disconnected;
                glashAgent.Dispose();
                glashAgent = null;
            }
        }
    }
}
