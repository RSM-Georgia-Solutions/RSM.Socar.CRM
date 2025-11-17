using Microsoft.AspNetCore.Authorization;
using RSM.Socar.CRM.Application.Security;
using RSM.Socar.CRM.Infrastructure.Security;
using RSM.Socar.CRM.Web.Security;

namespace RSM.Socar.CRM.Web.Extensions;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        // Permission evaluation (uses roles + user permissions)
        services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();

        // Authorization handler that checks permissions
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Custom policy provider to support "Permission:XYZ"
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Optional: central registry of available permissions
        services.AddSingleton<IPermissionRegistry, PermissionRegistry>();

        return services;
    }
}
