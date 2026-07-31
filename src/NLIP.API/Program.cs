using System.Text;
using AspNetCoreRateLimit;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLIP.API.Middleware;
using NLIP.API.Security;
using NLIP.Application;
using NLIP.Application.Common.Interfaces;
using NLIP.Infrastructure;
using NLIP.Infrastructure.Logging;
using NLIP.Infrastructure.Realtime;
using NLIP.Infrastructure.Security;
using NLIP.Integration;
using NLIP.Persistence;
using NLIP.Persistence.Seed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var isTesting = builder.Environment.IsEnvironment("Testing");

Log.Logger = SerilogConfigurator.Configure(builder.Configuration, "NLIP.API", enableDatabaseSink: !isTesting).CreateLogger();
builder.Host.UseSerilog();

// ---- Layers ----
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration, registerDbContext: !isTesting);
builder.Services.AddInfrastructure(builder.Configuration, useHangfire: !isTesting);
builder.Services.AddIntegration(builder.Configuration);

// ---- Auth: JWT bearer + dynamic permission-claim policies ----
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            string.IsNullOrWhiteSpace(jwtOptions.SigningKey) ? new string('0', 32) : jwtOptions.SigningKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    // Let SignalR clients pass the JWT via query string (?access_token=) since browsers can't
    // set an Authorization header on a WebSocket handshake.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddAuthorization();

builder.Services.AddSignalR();
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, NLIP.Infrastructure.Realtime.JwtUserIdProvider>();

// ---- Rate limiting (AspNetCoreRateLimit) ----
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ---- CORS: Blazor Web UI + any SPA client ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("NlipClients", policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NAICOM Life Assurance Integration Platform (NLIP) API",
        Version = "v1",
        Description = "Middleware API synchronizing Individual Life and Group Life policies between the Core Insurance Application and the NAICOM Portal."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<NlipDbContext>("database");

var app = builder.Build();

// ---- Startup: migrate + seed (idempotent). Skipped under the "Testing" environment, where
// NLIP.IntegrationTests' WebApplicationFactory swaps in an EnsureCreated() Sqlite database instead
// (no EF migration files exist yet in this scaffold — see docs/ROADMAP.md "Generate the initial
// EF Core migration" for why, and run `dotnet ef migrations add InitialCreate` before first deploy). ----
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NlipDbContext>();
    await db.Database.MigrateAsync();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbInitializer.SeedAsync(db, passwordHasher);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("NlipClients");
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

if (!isTesting)
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireDashboardAuthFilter() }
    });
}

app.Run();

/// <summary>Exposed for WebApplicationFactory-based integration tests (see tests/NLIP.IntegrationTests).</summary>
public partial class Program { }
