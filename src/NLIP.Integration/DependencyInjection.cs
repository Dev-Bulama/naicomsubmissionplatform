using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLIP.Application.Common.Interfaces;
using NLIP.Integration.Client;
using NLIP.Integration.Resilience;

namespace NLIP.Integration;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NaicomOptions>(configuration.GetSection(NaicomOptions.SectionName));
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Auth token client: separate typed client (no auth header, points at the same base
        // address) so requesting a token never recurses into the auth-token-provider itself.
        services.AddHttpClient<INaicomAuthTokenProvider, NaicomAuthTokenProvider>((sp, client) =>
        {
            var options = configuration.GetSection(NaicomOptions.SectionName).Get<NaicomOptions>() ?? new NaicomOptions();
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddHttpClient<INaicomApiClient, NaicomApiClient>((sp, client) =>
        {
            var options = configuration.GetSection(NaicomOptions.SectionName).Get<NaicomOptions>() ?? new NaicomOptions();
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan; // Polly's Timeout policy governs per-attempt timeout instead.
        }).AddPolicyHandler((sp, _) =>
        {
            var options = configuration.GetSection(NaicomOptions.SectionName).Get<NaicomOptions>() ?? new NaicomOptions();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("NLIP.Integration.NaicomResilience");
            return NaicomResiliencePolicies.Build(options, logger);
        });

        return services;
    }
}
