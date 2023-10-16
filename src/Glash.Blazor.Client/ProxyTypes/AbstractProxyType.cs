using Glash.Client;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using System.Net;
using System.Runtime.Versioning;

namespace Glash.Blazor.Client.ProxyTypes
{
    public abstract class AbstractProxyType<TUI> : IProxyType
    {
        public abstract string Icon { get; }
        public string Name
        {
            get
            {
                return ProxyTypeManager.Instance
                     .GetProxyTypeInfo(this.GetType().FullName)?
                     .Name;
            }
        }

        public abstract ProxyTypeButton[] GetButtons();

        public RenderFragment GetUI()
        {
            return Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<TUI>(
                new Dictionary<string, object>()
                {
                    ["Model"] = this
                });
        }

        protected string GetLocalIPAddress(string localIPAddress)
        {
            if (localIPAddress == IPAddress.Any.ToString())
                localIPAddress = IPAddress.Loopback.ToString();
            if (localIPAddress == IPAddress.IPv6Any.ToString())
                localIPAddress = IPAddress.IPv6Loopback.ToString();
            return localIPAddress;
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        protected IntPtr WaitForProcessMainWindow(Process process)
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (process.HasExited)
                    return IntPtr.Zero;
                var hWnd = process.MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                    return hWnd;
            }
        }
    }
}
