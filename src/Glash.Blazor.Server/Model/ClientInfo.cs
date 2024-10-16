﻿using Glash.Server;
using Quick.LiteDB.Plus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Glash.Blazor.Server.Model
{
    [Table($"{nameof(Glash)}_{nameof(Server)}_{nameof(ClientInfo)}")]
    public class ClientInfo
    {
        public ClientInfo() { }
        public ClientInfo(string name) { Name = name; }

        [Key]
        [LiteDB.BsonId]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        [NotMapped]
        [JsonIgnore]
        [LiteDB.BsonIgnore]
        public GlashClientContext Context { get; set; }

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
