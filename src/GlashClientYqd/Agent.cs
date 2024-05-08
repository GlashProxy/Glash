using Glash.Blazor.Client;
using Quick.EntityFrameworkCore.Plus;
using Quick.EntityFrameworkCore.Plus.SQLite;
using YiQiDong.Agent;
using YiQiDong.Core;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;

namespace GlashClientYqd;

public class Agent : AbstractAgent
{
    private WebApplication app;

    public override void Init()
    {
        Quick.Protocol.QpAllClients.RegisterUriSchema();
        base.Init();
    }

    public override void Start()
    {
        var dbFile = SQLiteDbContextConfigHandler.CONFIG_DB_FILE;
        if (!AgentContext.IsContainerRuning)
        {
            dbFile = Path.Combine(Path.GetDirectoryName(typeof(Agent).Assembly.Location), dbFile);
        }
        ConfigDbContext.Init(new SQLiteDbContextConfigHandler(dbFile), modelBuilder =>
        {
            Global.Instance.OnModelCreating(modelBuilder);
        });
        using (var dbContext = new ConfigDbContext())
            dbContext.DatabaseEnsureCreatedAndUpdated(t => AgentContext.LogInfo(t));
        ConfigDbContext.CacheContext.LoadCache();
        var version = AgentContext.Container?.Image?.Version;
        if (string.IsNullOrEmpty(version))
            version = "1.2.x";
        Global.Instance.Init(version);

#if DEBUG
        var webApplicationOptions = new WebApplicationOptions();
#else
            var webApplicationOptions = new WebApplicationOptions()
            {
                ContentRootPath = System.IO.Path.GetDirectoryName(typeof(Agent).Assembly.Location)
            };
#endif
        var builder = WebApplication.CreateBuilder(webApplicationOptions);
#if DEBUG
        builder.WebHost.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
#endif
        builder.Logging.ClearProviders();
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor()
            .AddCircuitOptions(options => { options.DetailedErrors = true; });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        app = builder.Build();
        if (builder.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        //启动Web服务
        try
        {
            app.Start();
            AgentContext.Client?.SendCommand(new YiQiDong.Protocol.V1.QpCommands.AddReverseProxyRule.Request()
            {
                Path = "/",
                Url = app.Urls.First(),
                Links = new[]
                {
                    new ReverseProxyRuleLinkInfo()
                    {
                        Name = "访问",
                        Url=""
                    }
                }
            })?.ContinueWith(task =>
            {
                if (task.IsFaulted)
                    AgentContext.LogWarn($"注册反向代理规则失败，原因：{ExceptionUtils.GetExceptionMessage(task.Exception.InnerException)}");
                else
                    AgentContext.LogInfo($"注册反向代理规则成功");
            });
            AgentContext.LogInfo($"Web服务启动完成，地址：{string.Join(",", app.Urls)}");
        }
        catch (Exception ex)
        {
            var message = $"启动Web服务时失败，原因：" + ex.Message;
            AgentContext.LogError(message);
            throw new Exception(message, ex);
        }
    }

    public override void Stop()
    {
        app?.StopAsync();
        app = null;
        base.Stop();        
    }
}
