using Quick.Blazor.Bootstrap;

namespace Glash.Blazor.Server.Pages
{
    public partial class LogsManage : IDisposable
    {
        private LogViewControl logViewControl;
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                foreach (var line in Global.Instance.Logs)
                    logViewControl.AddLine(line);
                Global.Instance.GlashServer.LogPushed += GlashServer_LogPushed;
            }
        }

        private void GlashServer_LogPushed(object sender, string e)
        {
            logViewControl.AddLine(e);
        }

        public void Dispose()
        {
            Global.Instance.GlashServer.LogPushed -= GlashServer_LogPushed;
        }
    }
}
