using Glash.Client;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Glash.Blazor.Client.ProxyTypes
{
    [ProxyType(typeof(Texts), nameof(Texts.ProxyTypeName))]
    public class RDP : AbstractProxyType<RDP_UI>
    {
        public enum Texts
        {
            ProxyTypeName,
            User,
            Password,
            ButtonStartRDP
        }

        [Required]
        public string User { get; set; }
        [Required]
        public string Password { get; set; }

        public override string Icon => "fa fa-windows";

        [SupportedOSPlatform("windows")]
        public override ProxyTypeButton[] GetButtons()
        {
            return new ProxyTypeButton[]
            {
                new ProxyTypeButton(
                    Global.Instance.TextManager.GetText(Texts.ButtonStartRDP),
                    "fa fa-desktop",
                    StartRDP
                    )
            };
        }

        /// <summary>
        /// 获取RDP密码
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static string GetRdpPassWord(string pw)
        {
            byte[] secret = Encoding.Unicode.GetBytes(pw);
            byte[] encryptedSecret = ProtectedData.Protect(secret, null, DataProtectionScope.LocalMachine);
            return BitConverter.ToString(encryptedSecret).Replace("-", string.Empty);
        }

        private string getRdpFileName(ProxyRuleContext t)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(t.Config.Name);
            sb.Append("-");
            sb.Append(t.Config.Agent);
            sb.Replace('.', '_');
            sb.Replace(' ', '_');
            foreach (var c in Path.GetInvalidFileNameChars())
                sb.Replace(c, '_');
            return sb.ToString();
        }

        [SupportedOSPlatform("windows")]
        private void StartRDP(ProxyRuleContext t)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"full address:s:{GetLocalIPAddress(t.Config.LocalIPAddress)}:{t.LocalPort}");
            sb.AppendLine($"username:s:{User}");
            sb.AppendLine($"password 51:b:{GetRdpPassWord(Password)}");
            var tmpFile = Path.Combine(Path.GetTempPath(), getRdpFileName(t));
            try
            {
                File.WriteAllText(tmpFile, sb.ToString());
                var process = Process.Start("mstsc.exe", tmpFile);
                WaitForProcessMainWindow(process);
                if (File.Exists(tmpFile))
                    File.Delete(tmpFile);
            }
            catch
            {
                File.Delete(tmpFile);
                throw;
            }
        }
    }
}
