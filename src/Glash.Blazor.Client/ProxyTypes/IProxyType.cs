using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.ProxyTypes
{
    public interface IProxyType
    {
        string Name { get; }
        string Icon { get; }
        public RenderFragment GetUI();
        public ProxyTypeButton[] GetButtons();
    }
}
