using AuthLibrary.Areas.Auth.Authentication;
using AuthLibrary.Areas.Auth.DAL;
using AuthLibrary.Areas.Auth.Extensions;
using BytexDigital.Blazor.Components.CookieConsent;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using OwaspTool.Components;
using OwaspTool.DAL;
using OwaspTool.EndPoints;
using OwaspTool.Services;
using OwaspTool.ViewModels;
using QuestPDF.Infrastructure;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddDbContext<OwaspToolContext>(item => item.UseSqlServer(builder.Configuration.GetConnectionString("conn")));
builder.Services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Authconn")));

builder.Services.AddTransient<IUserWebAppRepository, UserWebAppRepository>();
builder.Services.AddTransient<ISurveyRepository, SurveyRepository>();

// IMPORTANT: use Scoped for repositories that depend on a scoped DbContext
builder.Services.AddScoped<ISurveyRequirementsRepository, SurveyRequirementsRepository>();

builder.Services.AddTransient<IWebAppRegistryViewModel, WebAppRegistryViewModel>();

// Register IHttpClientFactory and a named client, then provide a scoped HttpClient from the factory
builder.Services.AddHttpClient("App", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AppUrl"]);
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("App"));

builder.Services.AddScoped<IProjectUserSyncService, ProjectUserSyncService>();
builder.Services.AddScoped<IRequirementsPdfGeneratorService, RequirementsPdfGeneratorService>();
builder.Services.AddScoped<ITestsPdfGeneratorService, TestPdfGeneratorService>();
builder.Services.AddScoped<NavigationService>();

// Data Protection configuration - ONLY FOR PRODUCTION - COMMENT OUT IN DEVELOPMENT
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo("D:\\Siti\\Siti\\OwaspTool\\keys"))
//    .SetApplicationName("CureCode");

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

// ONLY DEVELOPMENT ENVIRONMENT RIGHT NOW - COMMENT IN PRODUCTION
builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434");
});

// MVC / Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseStatusCodePages(context =>
{
    var statusCode = context.HttpContext.Response.StatusCode;

    if (statusCode == 404)
    {
        context.HttpContext.Response.Redirect("/error-404");
    }
    return Task.CompletedTask;
});

app.MapRazorPages();

app.UseAntiforgery();

app.MapSignOutEndpoint();
app.MapSurveyRequirementsEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
