
using Glash.Blazor.Client.Model;
using Quick.LiteDB.Plus;

namespace Glash.Blazor.Client;

public class ProfileContextManager
{
    private Dictionary<string, ProfileContext> profileContextDict;
    public static ProfileContextManager Instance { get; } = new ProfileContextManager();
    private ProfileContextManager()
    {
        profileContextDict = new Dictionary<string, ProfileContext>();

        var profileModels = ConfigDbContext.CacheContext.Query<Model.Profile>();
        foreach (var model in profileModels)
        {
            profileContextDict[model.Id] = new ProfileContext(model);
        }
    }

    public ProfileContext Get(string value)
    {
        if (profileContextDict.TryGetValue(value, out var profileContext))
            return profileContext;
        return null;
    }
    public ProfileContext[] GetProfileContexts() => profileContextDict.Values.ToArray();

    public void Add(Profile model)
    {
        ConfigDbContext.CacheContext.Add(model);
        profileContextDict[model.Id] = new ProfileContext(model);
    }

    public void Update(Profile model)
    {
        ConfigDbContext.CacheContext.Update(model);
    }

    public void Remove(Profile model)
    {
        ConfigDbContext.CacheContext.Remove(model, true);
        profileContextDict.Remove(model.Id);
    }
}
