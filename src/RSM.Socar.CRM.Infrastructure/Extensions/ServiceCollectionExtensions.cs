using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Application.Auth;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Infrastructure.Persistence.Interceptors;
using RSM.Socar.CRM.Infrastructure.Persistence.Repositories;
using RSM.Socar.CRM.Infrastructure.Security;
using RSM.Socar.CRM.Infrastructure.Seed;

namespace RSM.Socar.CRM.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Infrastructure layer: DbContext, options, security services, etc.
    /// </summary>
    /// <param name="services">DI</param>
    /// <param name="cfg">Configuration (expects ConnectionStrings:Sql and Jwt section)</param>
    /// <param name="configureDb">Optional extra EF options (provider, retries, migrations assembly)</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration cfg,
        Action<DbContextOptionsBuilder>? configureDb = null)
    {

        services.AddHttpContextAccessor();                   // needed for CurrentUser
        services.AddScoped<ICurrentUser, CurrentUser>();     // per request

        services.AddScoped<AuditingSoftDeleteInterceptor>(); // interceptor
        services.AddScoped<AuthorizationInterceptor>();


        // DbContext registration that supports both caller-provided config and the interceptor
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            // 1) Let the host decide the provider and extras, if they passed a delegate
            if (configureDb is not null)
            {
                configureDb(options);
            }
            else
            {
                // 2) Sensible default: SQL Server + retries + migrations assembly in Infrastructure
                options.UseSqlServer(
                    cfg.GetConnectionString("Sql"),
                    sql => {
                        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        sql.MigrationsAssembly("RSM.Socar.CRM.Infrastructure");
                    });
            }

            // 3) Always add the interceptor (auditing + soft-delete) after the provider is configured
            options.AddInterceptors(sp.GetRequiredService<AuditingSoftDeleteInterceptor>());

            //options.AddInterceptors(
            //    sp.GetRequiredService<AuditingSoftDeleteInterceptor>(),
            //    sp.GetRequiredService<AuthorizationInterceptor>()
            //);


            // (optional) nice-to-haves, driven by config
            var env = sp.GetRequiredService<IHostEnvironment>();
            if (env.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddScoped<IDbExecutionStrategy, DbExecutionStrategy>();
        services.AddScoped<IDbTransactionFactory, DbTransactionFactory>();

        services.AddScoped<IPermissionDiscoveryService, PermissionDiscoveryService>();
        services.AddScoped<RolePermissionSeeder>();

        // Users repository
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();


        // ---- Options ----
        services.Configure<JwtOptions>(cfg.GetSection(JwtOptions.SectionName));

        // ---- Services (no ASP.NET Core dependency needed) ----
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Password hasher (lightweight, from Microsoft.Extensions.Identity.Core)
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}
