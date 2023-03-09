using AssetServer.Models;
using AssetServer.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Serilog;
using Serilog.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(
    builder.Configuration.GetValue<string>("ApplicationUrl")
        ?? throw new InvalidOperationException("Missing $.ApplicationUrl appsettings parameter")
);

builder.Logging.ClearProviders();
builder.Host.UseSerilog(
    (ctx, config) =>
    {
        config.WriteTo.Console();
        config.WriteTo.Debug();
        config.MinimumLevel.Information();
        config.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning);
    }
);

builder.Services
    .AddOptions<AuthenticationOptions>()
    .BindConfiguration(nameof(AuthenticationOptions));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddScoped<IHostEnvironmentAuthenticationStateProvider>(
    sp => (ServerAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>()
);
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<FileRetrievalService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseSerilogRequestLogging(
    cfg =>
        cfg.GetLevel = (HttpContext ctx, double elapsed, Exception? ex) =>
            ctx.Response.StatusCode switch
            {
                < 400 => LogEventLevel.Information,
                >= 400 and < 500 => LogEventLevel.Warning,
                >= 500 => LogEventLevel.Error,
            }
);

app.UseStaticFiles();

app.MapGet("/ping", () => "pong");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Services.CreateScope().ServiceProvider.GetRequiredService<AuthenticationService>().IssueToken();

app.Run();
