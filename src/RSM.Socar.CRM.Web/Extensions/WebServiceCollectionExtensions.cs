using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Infrastructure.Security;
using RSM.Socar.CRM.Web.OData;
using RSM.Socar.CRM.Web.Swagger;
using System.Text;

namespace RSM.Socar.CRM.Web.Extensions;

public static class WebServiceCollectionExtensions
{
    // --- public composite ---
    public static IServiceCollection AddWebLayer(this IServiceCollection services, IConfiguration cfg)
        => services
            .AddJwtAuthentication(cfg)
            .AddCorsPolicy(cfg)
            .AddODataControllers()
            .AddSwaggerWithODataDelta()
            .AddHealthCheckLayer(cfg);

    // --- internals used by AddWebLayer ---
    public static IServiceCollection AddHealthCheckLayer(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddHealthChecks()
            // Liveness (“is the process up?”)
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })

            // Readiness (“can we serve traffic?”)
            .AddDbContextCheck<AppDbContext>("db", failureStatus: HealthStatus.Unhealthy, tags: new[] { "ready" });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration cfg)
    {
        var jwt = cfg.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration _)
    {
        // adjust origins later if needed
        services.AddCors(o => o.AddPolicy("Default", p =>
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
        return services;
    }

    public static IServiceCollection AddODataControllers(this IServiceCollection services)
    {
        services
            .AddControllers().AddOData(opt => opt
              .AddRouteComponents("odata", EdmModelBuilder.GetEdmModel(), services =>
              {
                  services.AddSingleton(new ODataValidationSettings
                  {
                      MaxTop = 100,
                      MaxAnyAllExpressionDepth = 5,
                      MaxNodeCount = 200
                  });
              })
              .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100));
        return services;
    }

    public static IServiceCollection AddSwaggerWithODataDelta(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your token}"
            });
            c.SchemaFilter<DeltaSchemaFilter>();
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme, Id = "Bearer" }}, new string[] {}
                }
            });
        });

        return services;
    }
}
