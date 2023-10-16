using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.ProxyTypes
{
    public abstract class AbstractProxyTypeUI<T> : ComponentBase
        where T : IProxyType, new()
    {
        [Parameter]
        public T Model { get; set; }
    }
}
