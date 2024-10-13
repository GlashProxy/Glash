using Quick.LiteDB.Plus;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glash.Blazor.Server.Model
{
    [Table($"{nameof(Glash)}_{nameof(Server)}_{nameof(ClientAgentRelation)}")]
    public class ClientAgentRelation : BaseModel,IHasDependcyRelation
    {
        public string ClientName { get; set; }
        public string AgentName { get; set; }

        public ModelDependcyInfo[] GetDependcyRelation()
        {
            return new ModelDependcyInfo[]
            {
                new ModelDependcyInfo<ClientAgentRelation, ClientInfo>
                (
                    source => source.ClientName == null ? null : new ClientInfo(source.ClientName),
                    target => t => t.ClientName == target.Name
                ),
                new ModelDependcyInfo<ClientAgentRelation, AgentInfo>
                (
                    source => source.AgentName == null ? null : new AgentInfo(source.AgentName),
                    target => t => t.AgentName == target.Name
                )
            };
        }
    }
}
