using Microsoft.EntityFrameworkCore;
using InfraSizingCalculator.Components;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Middleware;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using InfraSizingCalculator.Services.Pricing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add API controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// Add CORS for API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("Production", policy =>
    {
        // In production, restrict to specific origins
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add database context
builder.Services.AddDbContext<InfraSizingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddSingleton<CalculatorSettings>();
builder.Services.AddSingleton<IDistributionService, DistributionService>();
builder.Services.AddSingleton<ITechnologyService, TechnologyService>();
builder.Services.AddScoped<IK8sSizingService, K8sSizingService>();
builder.Services.AddScoped<IVMSizingService, VMSizingService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IWizardStateService, WizardStateService>();
builder.Services.AddScoped<ISettingsPersistenceService, SettingsPersistenceService>();
builder.Services.AddScoped<ConfigurationSharingService>();
builder.Services.AddScoped<ValidationRecommendationService>();

// Register centralized app state service (UI state management)
builder.Services.AddScoped<IAppStateService, AppStateService>();

// Register pricing and cost estimation services
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<ICostEstimationService, CostEstimationService>();
// Use database-backed pricing settings service
builder.Services.AddScoped<IPricingSettingsService, DatabasePricingSettingsService>();

// Register scenario services
builder.Services.AddScoped<IScenarioRepository, LocalStorageScenarioRepository>();
builder.Services.AddScoped<IScenarioService, ScenarioService>();

// Register growth planning service
builder.Services.AddScoped<IGrowthPlanningService, GrowthPlanningService>();

var app = builder.Build();

// Initialize database
if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.

// Global exception handler - must be early in the pipeline
app.UseGlobalExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    // HTTPS redirection in production
    app.UseHsts();
    app.UseHttpsRedirection();

    // Use production CORS policy
    app.UseCors("Production");
}
else
{
    // In development, allow all origins
    app.UseCors("AllowAll");
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
