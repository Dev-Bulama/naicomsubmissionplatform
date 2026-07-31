using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NLIP.Application.Common.Behaviors;

namespace NLIP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuditLoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        return services;
    }
}
