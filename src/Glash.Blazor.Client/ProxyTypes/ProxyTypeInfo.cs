using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.ProxyTypes
{
    public class ProxyTypeInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        private Func<IProxyType> factory;

        public IProxyType CreateInstance(string config = null)
        {
            IProxyType proxyType = factory.Invoke();
            if (!string.IsNullOrEmpty(config))
                JsonConvert.PopulateObject(config, proxyType);
            return proxyType;
        }

        internal ProxyTypeInfo(string id, string name, Func<IProxyType> factory)
        {
            Id = id;
            Name = name;
            this.factory = factory;
        }
    }
}
