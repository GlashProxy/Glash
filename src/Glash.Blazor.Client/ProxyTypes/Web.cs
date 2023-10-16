using Quick.Blazor.Bootstrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.ProxyTypes
{
    [ProxyType(typeof(Texts), nameof(Texts.ProxyTypeName))]
    public class Web : AbstractProxyType<Web_UI>
    {
        public enum Texts
        {
            ProxyTypeName,
            Schema,
            Path,
            ButtonVisit
        }

        public string Schema { get; set; }
        public string Path { get; set; }

        public override string Icon => "fa fa-globe";

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        public override ProxyTypeButton[] GetButtons()
        {
            return new ProxyTypeButton[]
            {
                new ProxyTypeButton(
                    Global.Instance.TextManager.GetText(Texts.ButtonVisit),
                    "fa fa-globe",
                    t=>
                    {
                        if(string.IsNullOrEmpty(Schema))
                            Schema="http";
                        var url = $"{Schema}://{GetLocalIPAddress(t.Config.LocalIPAddress)}:{t.LocalPort}/{Path}";
                        try
                        {
                            //先尝试使用Chrome浏览器打开
                            var psi = new ProcessStartInfo("chrome");
                            psi.ArgumentList.Add("-incognito");
                            psi.ArgumentList.Add(url);
                            psi.UseShellExecute = true;
                            var process = Process.Start(psi);
                            WaitForProcessMainWindow(process);
                        }
                        catch
                        {
                            //使用系统默认浏览器打开
                            var psi = new ProcessStartInfo(url);
                            psi.UseShellExecute = true;
                            var process = Process.Start(psi);;
                            WaitForProcessMainWindow(process);
                        }
                    })
            };
        }
    }
}
