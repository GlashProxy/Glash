using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glash.Client.Protocol.QpModel
{
    public class ProxyRuleInfo
    {
        public enum Texts
        {
            ModelName,
            TabBasic,
            TabProxyType,
            SelectOptionNone,
            Name,
            Agent,
            Local,
            LocalIPAddress,
            LocalPort,
            Remote,
            RemoteHost,
            RemotePort,
            ProxyType,
            ProxyTypeConfig
        }

        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Agent { get; set; }
        [Required]
        public string LocalIPAddress { get; set; }
        [Required]
        [Range(0, 65535)]
        public int LocalPort { get; set; }
        [Required]
        public string RemoteHost { get; set; }
        [Required]
        [Range(0, 65535)]
        public int RemotePort { get; set; }
        public string ProxyType { get; set; }
        public string ProxyTypeConfig { get; set; }
        [NotMapped]
        public bool Enable { get; set; } = false;

        public override string ToString()
        {
            return $"ProxyRule[{Name}]";
        }
    }
}
