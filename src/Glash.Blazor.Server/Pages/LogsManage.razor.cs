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
                logViewControl.SetContent(string.Join(Environment.NewLine, Global.Instance.Logs));
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
