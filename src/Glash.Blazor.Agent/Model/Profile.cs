using Quick.LiteDB.Plus;
using Quick.Localize;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glash.Blazor.Agent.Model
{
    [Table($"{nameof(Glash)}_{nameof(Agent)}_{nameof(Profile)}")]
    public class Profile : BaseModel
    {
        public Profile() { }
        public Profile(string id)
        {
            Id = id;
        }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ServerUrl { get; set; }
        public string AgentName { get; set; }
        public string AgentPassword { get; set; }

        public override string ToString()
        {
            return Locale.GetString("Profile") + $"[{Name}]";
        }
    }
}
