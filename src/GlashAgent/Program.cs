using Glash.Blazor.Agent;
using Glash.Blazor.Agent.Core;
using Quick.EntityFrameworkCore.Plus;
using Quick.EntityFrameworkCore.Plus.SQLite;
using System.Diagnostics;

Quick.Protocol.QpAllClients.RegisterUriSchema();
var dbFile = SQLiteDbContextConfigHandler.CONFIG_DB_FILE;
#if DEBUG
dbFile = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), dbFile);
#endif
ConfigDbContext.Init(new SQLiteDbContextConfigHandler(dbFile), modelBuilder =>
{
    Global.Instance.OnModelCreating(modelBuilder);
});
using (var dbContext = new ConfigDbContext())
    dbContext.DatabaseEnsureCreatedAndUpdated(t => Debug.Print(t));
ConfigDbContext.CacheContext.LoadCache();
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
