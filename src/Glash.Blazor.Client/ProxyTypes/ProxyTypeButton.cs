using Glash.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.ProxyTypes
{
    public class ProxyTypeButton
    {
        public string Name { get; private set; }
        public string Icon { get; private set; }
        public Action<ProxyRuleContext> Handler { get; private set; }

        public ProxyTypeButton(string name, string icon, Action<ProxyRuleContext> handler)
        {
            this.Name = name;
            this.Icon = icon;
            this.Handler = handler;
        }

        public void Invoke(ProxyRuleContext proxyRuleContext)
        {
            Handler.Invoke(proxyRuleContext);
        }
    }
}
