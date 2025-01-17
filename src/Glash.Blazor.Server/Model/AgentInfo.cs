﻿using Glash.Server;
using Quick.LiteDB.Plus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Glash.Blazor.Server.Model
{
    [Table($"{nameof(Glash)}_{nameof(Server)}_{nameof(AgentInfo)}")]
    public class AgentInfo
    {
        public AgentInfo() { }
        public AgentInfo(string name) { Name = name; }

        [Key]
        [Required]
        [MaxLength(100)]
        [LiteDB.BsonId]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        [NotMapped]
        [JsonIgnore]
        [LiteDB.BsonIgnore]
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
