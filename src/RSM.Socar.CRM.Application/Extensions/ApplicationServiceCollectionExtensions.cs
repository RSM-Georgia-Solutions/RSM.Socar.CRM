using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RSM.Socar.CRM.Application.Behaviors;

namespace RSM.Socar.CRM.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // -------------------------------------------------
        // 1. Register MediatR ONCE — FOR WHOLE APPLICATION
        // -------------------------------------------------
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));

        // -------------------------------------------------
        // 2. Register all FluentValidation validators ONCE
        // -------------------------------------------------
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);

        // -------------------------------------------------
        // 3. Pipeline behaviors (in correct order)
        // -------------------------------------------------
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 👉 Enable transaction behavior later once you're ready
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
