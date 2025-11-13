using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RSM.Socar.CRM.Application.Auth;
using RSM.Socar.CRM.Application.Behaviors;
using RSM.Socar.CRM.Application.Users.Commands;

namespace RSM.Socar.CRM.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LoginCommand>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateUserCommand>());
        services.AddValidatorsFromAssemblyContaining<CreateUserCommand>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
