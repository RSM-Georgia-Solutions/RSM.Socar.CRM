using RSM.Socar.CRM.Web.Logging;

namespace RSM.Socar.CRM.Web.Extensions;

public static class BodyLoggingExtensions
{
    public static IServiceCollection AddRequestResponseBodyLogging(this IServiceCollection services, IConfiguration cfg)
    {
        services.Configure<RequestResponseLoggingOptions>(cfg.GetSection("RequestResponseLogging"));
        services.AddTransient<RequestResponseLoggingMiddleware>();
        return services;
    }

    // ✅ Return the same WebApplication instance — DO NOT return UseMiddleware(...)
    public static WebApplication UseRequestResponseBodyLogging(this WebApplication app)
    {
        app.UseMiddleware<RequestResponseLoggingMiddleware>();
        return app;
    }
}
