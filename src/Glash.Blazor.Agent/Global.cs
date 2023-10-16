using Microsoft.EntityFrameworkCore;
using Quick.Localize;

namespace Glash.Blazor.Agent
{
    public class Global
    {
        public static Global Instance { get; } = new Global();
        public TextManager TextManager { get; private set; }

        public Global()
        {
            TextManager = TextManager.DefaultInstance;
        }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.Profile>();
        }

        public void ChangeLanguage(string language)
        {
            TextManager = TextManager.GetInstance(language);
        }
    }
}
