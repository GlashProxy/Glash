using Quick.EntityFrameworkCore.Plus.SQLite;
using Quick.EntityFrameworkCore.Plus;
using Glash.Blazor.Server;
using System.Diagnostics;

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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();
app.UseWebSockets();
app.UseGlashServer("/glash", Global.Instance.ConnectionPassword);

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
