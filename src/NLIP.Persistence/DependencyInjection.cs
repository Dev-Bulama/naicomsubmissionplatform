using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLIP.Application.Common.Interfaces;
using NLIP.Persistence.Interceptors;
using NLIP.Persistence.Repositories;

namespace NLIP.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DispatchDomainEventsInterceptor>();

        services.AddDbContext<NlipDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null));
            options.AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<NlipDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
