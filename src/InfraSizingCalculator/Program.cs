using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using InfraSizingCalculator.Components;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Identity;
using InfraSizingCalculator.Middleware;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Auth;
using InfraSizingCalculator.Services.Interfaces;
using InfraSizingCalculator.Services.Pricing;
using InfraSizingCalculator.Services.Telemetry;
using InfraSizingCalculator.Services.HealthChecks;
using InfraSizingCalculator.Services.Validation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

// Configure Serilog early for bootstrap logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .WriteTo.File(
        formatter: new RenderedCompactJsonFormatter(),
        path: "logs/infrasizing-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting Infrastructure Sizing Calculator");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for all logging
builder.Host.UseSerilog();

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

// Add OpenTelemetry for observability
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "InfraSizingCalculator",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(CalculatorMetrics.MeterName)
        .AddConsoleExporter())  // Console exporter for development
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(CalculatorMetrics.MeterName)
        .AddPrometheusExporter());  // Exposes /metrics endpoint

// Register custom metrics service
builder.Services.AddSingleton<CalculatorMetrics>();

// Add health checks with custom checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "ready" })
    .AddCheck<DistributionServiceHealthCheck>("distribution-service", tags: new[] { "service", "ready" })
    .AddCheck<TechnologyServiceHealthCheck>("technology-service", tags: new[] { "service", "ready" });

// Add database context
builder.Services.AddDbContext<InfraSizingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity database context (separate from app data)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection") ?? "Data Source=identity.db"));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure authentication cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "InfraSizing.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

// Register authentication service
builder.Services.AddScoped<IAuthService, AuthService>();

// Register application services
builder.Services.AddSingleton<CalculatorSettings>();
builder.Services.AddSingleton<IDistributionService, DistributionService>();
builder.Services.AddSingleton<ITechnologyService, TechnologyService>();
builder.Services.AddSingleton<ITechnologyTemplateService, TechnologyTemplateService>();
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

// Register input validation service (Phase 2: Security)
builder.Services.AddScoped<IInputValidationService, InputValidationService>();

// Add rate limiting (Phase 2: Security)
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter - 100 requests per minute per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    // API-specific rate limiter - more restrictive for API endpoints
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 50;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken: token);

        Log.Warning("Rate limit exceeded for IP: {IP}",
            context.HttpContext.Connection.RemoteIpAddress);
    };
});

var app = builder.Build();

// Initialize databases
if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();

    // Initialize application database
    var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();
    dbContext.Database.EnsureCreated();

    // Initialize Identity database
    var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    identityDbContext.Database.EnsureCreated();

    Log.Information("Databases initialized successfully");
}

// Configure the HTTP request pipeline.

// Global exception handler - must be early in the pipeline
app.UseGlobalExceptionHandler();

// Security headers - add early in pipeline (Phase 2)
app.UseSecurityHeaders();

// Rate limiting - protect against abuse (Phase 2)
app.UseRateLimiter();

// Security settings from configuration (defaults to false for safe HTTP deployment)
var enableHsts = app.Configuration.GetValue<bool>("Security:EnableHsts", false);
var enableHttpsRedirection = app.Configuration.GetValue<bool>("Security:EnableHttpsRedirection", false);

if (!app.Environment.IsDevelopment())
{
    // HTTPS redirection only if explicitly enabled in config
    if (enableHsts)
    {
        app.UseHsts();
    }
    if (enableHttpsRedirection)
    {
        app.UseHttpsRedirection();
    }

    // Use production CORS policy if origins are configured, otherwise allow all
    var corsOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    if (corsOrigins != null && corsOrigins.Length > 0)
    {
        app.UseCors("Production");
    }
    else
    {
        app.UseCors("AllowAll");
    }
}
else
{
    // In development, allow all origins
    app.UseCors("AllowAll");
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Authentication & Authorization - must be before endpoint mapping
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Health check endpoints with detailed output
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Readiness check (for orchestrators like Kubernetes)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Liveness check (basic alive check)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // Excludes all checks, just returns 200 if app is running
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Prometheus metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
