using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RSM.Socar.CRM.Infrastructure.Security;
using RSM.Socar.CRM.Web.OData;
using RSM.Socar.CRM.Web.Swagger;

namespace RSM.Socar.CRM.Web.Extensions;

public static class WebServiceCollectionExtensions
{
    // --- public composite ---
    public static IServiceCollection AddWebLayer(this IServiceCollection services, IConfiguration cfg)
        => services
            .AddJwtAuthentication(cfg)
            .AddCorsPolicy(cfg)
            .AddODataControllers()
            .AddSwaggerWithODataDelta();

    // --- internals used by AddWebLayer ---

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
            .AddControllers()
            .AddOData(opt =>
                opt.AddRouteComponents("odata", EdmModelBuilder.GetEdmModel())
                   .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100));
        return services;
    }

    public static IServiceCollection AddSwaggerWithODataDelta(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => c.SchemaFilter<DeltaSchemaFilter>());
        return services;
    }
}
