using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Application.Auth;
using RSM.Socar.CRM.Application.Users;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Infrastructure.Persistence.Repositories;
using RSM.Socar.CRM.Infrastructure.Security;

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
        // ---- DbContext ----
        services.AddDbContext<AppDbContext>(options =>
        {
            if (configureDb is not null)
            {
                // Let the host decide the provider and extras
                configureDb(options);
            }
            else
            {
                // Sensible default: SQL Server + retries
                options.UseSqlServer(
                    cfg.GetConnectionString("Sql"),
                    sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // Users repository
        services.AddScoped<IUserRepository, UserRepository>();

        // ---- Options ----
        services.Configure<JwtOptions>(cfg.GetSection(JwtOptions.SectionName));

        // ---- Services (no ASP.NET Core dependency needed) ----
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Password hasher (lightweight, from Microsoft.Extensions.Identity.Core)
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}
