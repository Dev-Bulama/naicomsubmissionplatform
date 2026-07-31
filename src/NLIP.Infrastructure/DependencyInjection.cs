using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLIP.Application.Common.Interfaces;
using NLIP.Infrastructure.Caching;
using NLIP.Infrastructure.Jobs;
using NLIP.Infrastructure.Notifications;
using NLIP.Infrastructure.Security;
using NLIP.Infrastructure.Services;

namespace NLIP.Infrastructure;

public static class DependencyInjection
{
    /// <param name="useHangfire">
    /// False under the "Testing" environment (see NLIP.IntegrationTests.NlipWebApplicationFactory)
    /// so the test host never tries to open a real SQL Server connection for Hangfire storage —
    /// Hangfire's UseSqlServerStorage connects eagerly at registration time, which would otherwise
    /// make every integration test fail before a single request is handled. A no-op scheduler is
    /// registered instead so IBackgroundJobScheduler still resolves.
    /// </param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool useHangfire = true)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<EncryptionOptions>(configuration.GetSection(EncryptionOptions.SectionName));
        services.AddSingleton<SettingEncryptionService>();
        services.AddScoped<ISettingsService, SettingsService>();

        AddCaching(services, configuration);
        AddNotifications(services);

        if (useHangfire)
            AddHangfireClient(services, configuration);
        else
            services.AddScoped<IBackgroundJobScheduler, Jobs.NoOpBackgroundJobScheduler>();

        return services;
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "NLIP:";
            });
        }
        else
        {
            // Redis not configured (e.g. local dev) — falls back to an in-process distributed
            // cache implementing the same IDistributedCache contract, so ICacheService behaves
            // identically either way.
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, DistributedCacheService>();
    }

    private static void AddNotifications(IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<ISmsSender, NoOpSmsSender>();
        services.AddSingleton<ITeamsNotifier, NoOpTeamsNotifier>();
        services.AddSingleton<ISlackNotifier, NoOpSlackNotifier>();
    }

    /// <summary>
    /// Registers Hangfire storage + client (IBackgroundJobClient/IRecurringJobManager) so any
    /// host can enqueue jobs. Only NLIP.Worker additionally calls AddHangfireServer() to actually
    /// execute them — the API/Web hosts stay enqueue-only so a slow NAICOM call never ties up a
    /// web request thread.
    /// </summary>
    private static void AddHangfireClient(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        services.AddScoped<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();
    }
}
