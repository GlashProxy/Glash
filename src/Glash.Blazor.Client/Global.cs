using Glash.Blazor.Client.ProxyTypes;
using Glash.Client;
using Microsoft.EntityFrameworkCore;
using Quick.Blazor.Bootstrap;
using Quick.Localize;
using System.Globalization;

namespace Glash.Blazor.Client
{
    public class Global
    {
        public static Global Instance { get; } = new Global();

        private const string LANGUAGE_RESOURCE_PREFIX = "Glash.Blazor.Client.Language.";

        public string Version { get; private set; }
        private string _Language;
        public string Language
        {
            get { return _Language; }
            set
            {
                _Language = value;
                Blazor.Client.Model.Config.SetConfig(nameof(Language), value);
                afterLanuageChanged();
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler LanguageChanged;
        public event EventHandler ProfileChanged;
        public TextManager TextManager { get; private set; }
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

        public CultureInfo[] GetLanuages()
        {
            var list = new HashSet<string>();
            foreach (var t in typeof(Global).Assembly.GetManifestResourceNames())
            {
                if (!t.StartsWith(LANGUAGE_RESOURCE_PREFIX))
                    continue;
                var name = t.Substring(LANGUAGE_RESOURCE_PREFIX.Length);
                var index = name.IndexOf('.');
                if (index <= 0)
                    continue;
                name = name.Substring(0, index).Replace("_", "-");
                if (!list.Contains(name))
                    list.Add(name);
            }
            return list.Select(t => new CultureInfo(t)).ToArray();
        }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.Config>();
            modelBuilder.Entity<Model.Profile>();
        }

        public void Init(string version)
        {
            Version = version;
            _Language = Model.Config.GetConfig(nameof(Language));
            if (_Language == null)
                _Language = Thread.CurrentThread.CurrentCulture.IetfLanguageTag;
            afterLanuageChanged();            
        }

        private void afterLanuageChanged()
        {
            TextManager = TextManager.GetInstance(Language);
            ModalPrompt.TextOk = ModalAlert.TextOk = TextManager.GetText(Blazor.Client.ClientTexts.Ok);
            ModalPrompt.TextCancel = ModalAlert.TextCancel = TextManager.GetText(Blazor.Client.ClientTexts.Cancel);
            ProxyTypeManager.Instance.Init();
        }
    }
}
