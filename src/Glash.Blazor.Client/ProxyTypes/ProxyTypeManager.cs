using System.Reflection;

namespace Glash.Blazor.Client.ProxyTypes
{
    public class ProxyTypeManager
    {
        public static ProxyTypeManager Instance { get; } = new ProxyTypeManager();
        private Dictionary<string, ProxyTypeInfo> proxyTypeDict;

        public ProxyTypeInfo GetProxyTypeInfo(string proxyTypeId)
        {
            proxyTypeDict.TryGetValue(proxyTypeId, out ProxyTypeInfo proxyTypeInfo);
            return proxyTypeInfo;
        }

        public ProxyTypeInfo[] GetProxyTypeInfos() => proxyTypeDict.Values.ToArray();

        public void Init()
        {
            proxyTypeDict = new Dictionary<string, ProxyTypeInfo>();
            var getTextMethod = typeof(Quick.Localize.TextManager)
                .GetMethod(nameof(Quick.Localize.TextManager.GetText), new Type[] { typeof(string) });
            foreach (var type in GetType().Assembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract)
                    continue;
                if (!typeof(IProxyType).IsAssignableFrom(type))
                    continue;
                var id = type.FullName;
                var proxyTypeAttr = type.GetCustomAttribute<ProxyTypeAttribute>();
                var name = getTextMethod.MakeGenericMethod(proxyTypeAttr.NameEnumType)
                    .Invoke(Global.Instance.TextManager, new object[] { proxyTypeAttr.NameEnumName })
                    .ToString();
                proxyTypeDict[id] = new ProxyTypeInfo(
                    id,
                    name,
                    () => (IProxyType)Activator.CreateInstance(type));
            }
        }
    }
}
