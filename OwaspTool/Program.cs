using AuthLibrary.Areas.Auth.Authentication;
using AuthLibrary.Areas.Auth.DAL;
using AuthLibrary.Areas.Auth.Extensions;
using BytexDigital.Blazor.Components.CookieConsent;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using OwaspTool.Components;
using OwaspTool.DAL;
using OwaspTool.Services;
using System.Globalization;

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

builder.Services.AddScoped<IProjectUserSyncService, ProjectUserSyncService>();

builder.Services.AddAuthLibrary();

builder.Services.AddControllers();

builder.Services.AddRazorPages();

var defaultCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

builder.Services.AddCookieConsent(o =>
{
    o.Revision = 1;
    o.PolicyUrl = "/cookie-policy";

    // Call optional
    o.UseDefaultConsentPrompt(prompt =>
    {
        prompt.Position = ConsentModalPosition.BottomRight;
        prompt.Layout = ConsentModalLayout.Bar;
        prompt.SecondaryActionOpensSettings = false;
        prompt.AcceptAllButtonDisplaysFirst = false;
    });

    o.Categories.Add(new CookieCategory
    {
        TitleText = new()
        {
            ["en"] = "Persistent Cookies"
        },
        DescriptionText = new()
        {
            ["en"] = "These cookies allow the website to remember your login on future visits after you close your browser. They store a persistent authentication token, so that you don't have to log in again every time. Without these cookies the 'Remember me' feature cannot work."
        },
        Identifier = "persistent-cookies",
        IsPreselected = false
    });

});

builder.Services.AddCookieConsentHttpContextServices();

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

app.MapRazorPages();

app.UseAntiforgery();

app.MapSignOutEndpoint();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
