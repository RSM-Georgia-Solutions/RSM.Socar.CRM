using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RSM.Socar.CRM.Application.Auth;

namespace RSM.Socar.CRM.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LoginCommand>());
        return services;
    }
}
