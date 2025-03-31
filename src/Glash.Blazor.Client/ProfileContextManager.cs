
using Glash.Blazor.Client.Model;
using Quick.LiteDB.Plus;

namespace Glash.Blazor.Client;

public class ProfileContextManager
{
    private Dictionary<string, ProfileContext> profileDict;
    public static ProfileContextManager Instance { get; } = new ProfileContextManager();
    private ProfileContextManager()
    {
        profileDict = new Dictionary<string, ProfileContext>();

        var profileModels = ConfigDbContext.CacheContext.Query<Model.Profile>();
        foreach (var model in profileModels)
        {
            profileDict[model.Id] = new ProfileContext(model);
        }
    }

    public ProfileContext Get(string value)
    {
        if (profileDict.TryGetValue(value, out var profileContext))
            return profileContext;
        return null;
    }
    public ProfileContext[] GetProfileContexts() => profileDict.Values.ToArray();

    public void Add(Profile model)
    {
        lock (profileDict)
        {
            ConfigDbContext.CacheContext.Add(model);
            profileDict[model.Id] = new ProfileContext(model);
        }
    }

    public void Update(Profile model)
    {
        lock (profileDict)
        {
            ConfigDbContext.CacheContext.Update(model);
        }
    }

    public void Remove(Profile model)
    {
        lock (profileDict)
        {
            ConfigDbContext.CacheContext.Remove(model, true);
            profileDict.Remove(model.Id);
        }
    }

    public ProfileContext GetContext(Profile model)
    {
        lock (profileDict)
        {
            if (profileDict.TryGetValue(model.Id, out var context))
                return context;
        }
        return null;
    }
}
