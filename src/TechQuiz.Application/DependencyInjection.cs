using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TechQuiz.Application.Common.Behaviors;

namespace TechQuiz.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Pipeline order matters: outermost behavior runs first on the way in,
            // last on the way out. Logging wraps Validation wraps the handler so
            // that validation failures are logged.
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // TimeProvider is consumed by Application handlers (Start/Submit/Complete attempt
        // timestamps). Registered here so Application carries its own clock dependency
        // regardless of which host wires AddApplication.
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
