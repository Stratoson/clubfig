using Clubfig.Infrastructure.Data;
using Clubfig.Infrastructure.Repositories;
using ClubFig.Web;
using ClubFig.Web.Components;
using ClubFig.Web.Middleware;
using ClubFig.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register data access
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Register repositories
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Register tenant context (scoped per request)
builder.Services.AddScoped<TenantContext>();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.UseMiddleware<TenantResolutionMiddleware>();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
