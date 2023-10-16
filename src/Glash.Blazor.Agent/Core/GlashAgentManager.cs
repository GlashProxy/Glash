using Quick.EntityFrameworkCore.Plus;

namespace Glash.Blazor.Agent.Core
{
    public class GlashAgentManager
    {
        public static GlashAgentManager Instance { get; } = new GlashAgentManager();
        private Dictionary<Model.Profile, ProfileContext> profileDict = new Dictionary<Model.Profile, ProfileContext>();
        public ProfileContext[] ProfileContexts { get; private set; } = new ProfileContext[0];

        public void Init()
        {
            var profiles = ConfigDbContext.CacheContext.Query<Model.Profile>();
            foreach (var profile in profiles)
                OnAdd(profile);
        }

        public void OnAdd(Model.Profile model)
        {
            lock (profileDict)
            {
                profileDict[model] = new ProfileContext(model);
                ProfileContexts = profileDict.Values.ToArray();
            }
        }

        public void OnDelete(Model.Profile model)
        {
            lock (profileDict)
            {
                if (!profileDict.ContainsKey(model))
                    return;
                var serverContext = profileDict[model];
                serverContext.Dispose();
                profileDict.Remove(model);
                ProfileContexts = profileDict.Values.ToArray();
            }
        }
    }
}
