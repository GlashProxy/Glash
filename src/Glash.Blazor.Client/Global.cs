using Glash.Client;
using Microsoft.EntityFrameworkCore;

namespace Glash.Blazor.Client
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
