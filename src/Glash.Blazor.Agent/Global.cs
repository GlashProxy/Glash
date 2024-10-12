
using Quick.LiteDB.Plus;

namespace Glash.Blazor.Agent
{
    public class Global
    {
        public static Global Instance { get; } = new Global();

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.Config>(c => c.EnsureIndex(t => t.Id, true));
            modelBuilder.Entity<Model.Profile>(c => c.EnsureIndex(t => t.Id, true));
        }
    }
}
