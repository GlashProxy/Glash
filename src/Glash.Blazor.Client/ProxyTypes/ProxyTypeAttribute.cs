using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.ProxyTypes
{
    public class ProxyTypeAttribute : Attribute
    {
        public Type NameEnumType { get; set; }
        public string NameEnumName { get; set; }
        public ProxyTypeAttribute(Type nameEnumType, string nameEnumName)
        {
            NameEnumType = nameEnumType;
            NameEnumName = nameEnumName;
        }
    }
}
