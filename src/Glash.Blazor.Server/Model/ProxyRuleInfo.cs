using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glash.Blazor.Server.Model
{
    [Table($"{nameof(Glash)}_{nameof(Server)}_{nameof(ProxyRuleInfo)}")]
    public class ProxyRuleInfo : Client.Protocol.QpModel.ProxyRuleInfo, IHasDependcyRelation
    {
        public ProxyRuleInfo() { }
        public ProxyRuleInfo(string id)
        {
            Id = id;
        }

        public string ClientName { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj, t =>
                t.Id);
        }

        public override int GetHashCode()
        {
            return this.GetHashCode(t => t.Id);
        }

        public ModelDependcyInfo[] GetDependcyRelation()
        {
            return new ModelDependcyInfo[]
            {
                new ModelDependcyInfo<ProxyRuleInfo, ClientInfo>
                (
                    source => source.ClientName == null ? null : new ClientInfo(source.ClientName),
                    target => t => t.ClientName == target.Name
                )
            };
        }
    }
}
