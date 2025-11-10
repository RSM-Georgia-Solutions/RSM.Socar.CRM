using RSM.Socar.CRM.Web.Errors;

namespace RSM.Socar.CRM.Web.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandlingLayer(this IServiceCollection services, IConfiguration cfg)
    {
        services.Configure<ExceptionHandlingOptions>(cfg.GetSection("ExceptionHandling"));
        services.AddTransient<ExceptionHandlingMiddleware>();
        return services;
    }

    public static WebApplication UseExceptionHandlingLayer(this WebApplication app)
    {
        // This should be one of the first middlewares, so we catch everything after.
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
