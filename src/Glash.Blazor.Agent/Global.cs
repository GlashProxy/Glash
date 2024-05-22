using Microsoft.EntityFrameworkCore;
using Quick.Localize;

namespace Glash.Blazor.Agent
{
    public class Global
    {
        public static Global Instance { get; } = new Global();

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.Config>();
            modelBuilder.Entity<Model.Profile>();
        }
    }
}
