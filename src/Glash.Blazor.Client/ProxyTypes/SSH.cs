using Microsoft.Win32;
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
                        try
                        {
                            var process = Process.Start(Terminal,$"-ssh -l {User} -pw {Password} -P {t.LocalPort} {GetLocalIPAddress(t.Config.LocalIPAddress)}");
                            WaitForProcessMainWindow(process);
                        }
                        catch (System.ComponentModel.Win32Exception ex)
                        {
                            throw new IOException("未检测到PuTTY，请安装PuTTY！",ex);
                        }
                    }),
                new ProxyTypeButton(
                    Global.Instance.TextManager.GetText(Texts.ButtonStartFileTransfer),
                    "fa fa-folder",
                    t=>
                    {
#pragma warning disable CA1416 // 验证平台兼容性
                        //从注册表中读取NSIS的安装目录
                        var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\winscp3_is1", false);
                        if (regKey == null)
                        {
                            Console.WriteLine("未检测到WinSCP，请安装WinSCP！");
                            return;
                        }
                        var installLocation = regKey.GetValue("InstallLocation").ToString();
                        var exeFile = Path.Combine(installLocation, "WinSCP.exe");
#pragma warning restore CA1416 // 验证平台兼容性
                        var process = Process.Start(exeFile, $"/ini=nul sftp://{GetLocalIPAddress(t.Config.LocalIPAddress)}:{t.LocalPort}/ -username={User} -password={Password}");
                        WaitForProcessMainWindow(process);
                    })
            };
        }
    }
}
