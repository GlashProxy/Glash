using Glash.Server;
using Newtonsoft.Json;
using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glash.Blazor.Server.Model
{
    [Table($"{nameof(Glash)}_{nameof(Server)}_{nameof(AgentInfo)}")]
    public class AgentInfo
    {
        public enum Texts
        {
            ModelName,
            Name,
            Password,
            ChannelName,
            ConnectTime
        }

        public AgentInfo() { }
        public AgentInfo(string name) { Name = name; }

        [Key]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        [NotMapped]
        [JsonIgnore]
        public GlashAgentContext Context { get; set; }

        public override int GetHashCode()
        {
            return this.GetHashCode(t => t.Name);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj, t => t.Name);
        }
    }
}
