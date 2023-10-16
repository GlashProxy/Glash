using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.ProxyTypes
{
    [ProxyType(typeof(Texts), nameof(Texts.ProxyTypeName))]
    public class SSH : AbstractProxyType<SSH_UI>
    {
        public enum Texts
        {
            ProxyTypeName,
            User,
            Password,
            Terminal,
            ButtonStartTerminal,
            ButtonStartFileTransfer
        }
        [Required]
        public string User { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Terminal { get; set; }

        private const string NATIVE_FOLDER = "runtimes/win-x64/native";
        public override string Icon => "fa fa-linux";

        [SupportedOSPlatform("windows")]
        public override ProxyTypeButton[] GetButtons()
        {
            return new ProxyTypeButton[]
            {
                new ProxyTypeButton(
                    Global.Instance.TextManager.GetText(Texts.ButtonStartTerminal),
                    "fa fa-terminal",
                    t=>
                    {
                        if(string.IsNullOrEmpty(Terminal))
                            Terminal="putty";
                        var process = Process.Start($"{NATIVE_FOLDER}/PuTTY/{Terminal}",$"-ssh -l {User} -pw {Password} -P {t.LocalPort} {GetLocalIPAddress(t.Config.LocalIPAddress)}");
                        WaitForProcessMainWindow(process);
                    }),
                new ProxyTypeButton(
                    Global.Instance.TextManager.GetText(Texts.ButtonStartFileTransfer),
                    "fa fa-folder",
                    t=>
                    {
                        var process = Process.Start($"{NATIVE_FOLDER}/WinSCP/WinSCP", $"/ini=nul sftp://{GetLocalIPAddress(t.Config.LocalIPAddress)}:{t.LocalPort}/ -username={User} -password={Password}");
                        WaitForProcessMainWindow(process);
                    })
            };
        }
    }
}
