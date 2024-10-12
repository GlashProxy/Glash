using Glash.Blazor.Agent;
using Glash.Blazor.Agent.Core;
using Quick.LiteDB.Plus;
using System.Diagnostics;

Quick.Protocol.QpAllClients.RegisterUriSchema();
var dbFile = "Config.litedb";
#if DEBUG
dbFile = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), dbFile);
#endif
ConfigDbContext.Init(dbFile, modelBuilder =>
{
    Global.Instance.OnModelCreating(modelBuilder);
});
ConfigDbContext.CacheContext.LoadCache();
GlashAgent.Core.LoginPasswordManager.Instance.Init();
GlashAgentManager.Instance.Init();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
