using Glash.Agent;

namespace Glash.Blazor.Agent.Core
{
    public class ProfileContext : IDisposable
    {
        private CancellationTokenSource cts;
        private GlashAgent glashAgent;
        public Model.Profile Model { get; private set; }
        public string Status { get; private set; }

        public ProfileContext(Model.Profile model)
        {
            Model = model;
            cts = new CancellationTokenSource();
            glashAgent = new GlashAgent(Model.ServerUrl, Model.AgentName, Model.AgentPassword);
            glashAgent.Disconnected += GlashAgent_Disconnected;
            _ = beginConnect(cts.Token);
        }

        private void GlashAgent_Disconnected(object sender, EventArgs e)
        {
            Status = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {Global.Instance.TextManager.GetText(Agent.Model.Profile.Texts.Disconnected)}";
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
                Status = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {Global.Instance.TextManager.GetText(Agent.Model.Profile.Texts.Connected)}";
            }
            catch (Exception ex)
            {
                Status = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {ex.Message}";
                _ = delayToConnect(token);
                return;
            }
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts = null;
            glashAgent?.Dispose();
            glashAgent = null;
        }
    }
}
