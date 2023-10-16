using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glash.Blazor.Client.Model
{
    [Table($"{nameof(Glash)}_{nameof(Client)}_{nameof(Profile)}")]
    public class Profile : BaseModel
    {
        public enum Texts
        {
            ModelName,
            Name,
            ServerUrl,
            ClientName,
            ClientPassword
        }

        public Profile() { }
        public Profile(string id)
        {
            Id = id;
        }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ServerUrl { get; set; }
        public string ClientName { get; set; }
        public string ClientPassword { get; set; }

        public override string ToString()
        {
            return $"{Global.Instance.TextManager.GetText(Texts.ModelName)}[{Name}]";
        }
    }
}
