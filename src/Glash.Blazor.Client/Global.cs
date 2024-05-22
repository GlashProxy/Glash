using Glash.Client;
using Microsoft.EntityFrameworkCore;

namespace Glash.Blazor.Client
{
    public class Global
    {
        public static Global Instance { get; } = new Global();
        public string Version { get; private set; }
        public event EventHandler ProfileChanged;
        private Model.Profile _Profile;
        public Model.Profile Profile
        {
            get
            {
                return _Profile;
            }

            internal set
            {
                _Profile = value;
                ProfileChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public GlashClient GlashClient { get; internal set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.Config>();
            modelBuilder.Entity<Model.Profile>();
        }

        public void Init(string version)
        {
            Version = version;
        }
    }
}
