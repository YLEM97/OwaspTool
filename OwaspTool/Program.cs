using AuthLibrary.Areas.Auth.Authentication;
using AuthLibrary.Areas.Auth.DAL;
using AuthLibrary.Areas.Auth.Extensions;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using OwaspTool.Components;
using OwaspTool.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddDbContext<OwaspToolContext>(item => item.UseSqlServer(builder.Configuration.GetConnectionString("conn")));
builder.Services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Authconn")));

builder.Services.AddScoped<HttpClient>(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["AppUrl"])
});

builder.Services.AddAuthLibrary();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseAntiforgery();

app.MapSignOutEndpoint();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
