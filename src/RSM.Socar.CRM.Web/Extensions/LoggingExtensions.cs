using Serilog;
using Serilog.Exceptions; // exception detail enricher

namespace RSM.Socar.CRM.Web.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Configure Serilog for the host using appsettings (with sane defaults if missing).
    /// Call this on the builder, before Build().
    /// </summary>
    public static WebApplicationBuilder AddLoggingLayer(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, services, cfg) =>
        {
            cfg
                // Read from appsettings.json / appsettings.*.json -> "Serilog" section
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)

                // Enrich
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()

                // Fallback sink if none provided in config
                .WriteTo.Console();
        });

        return builder;
    }

    /// <summary>
    /// Adds Serilog request logging with useful fields and a concise template.
    /// Call this early in the pipeline (before your auth/handlers).
    /// </summary>
    public static WebApplication UseRequestLoggingLayer(this WebApplication app)
    {
        app.UseSerilogRequestLogging(opts =>
        {
            opts.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            opts.EnrichDiagnosticContext = (diag, http) =>
            {
                diag.Set("ClientIP", http.Connection.RemoteIpAddress?.ToString());
                diag.Set("QueryString", http.Request?.QueryString.Value);
                diag.Set("UserId", http.User?.FindFirst("sub")?.Value ?? http.User?.Identity?.Name);
                diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
                diag.Set("TraceId", http.TraceIdentifier);
            };
        });
        return app;
    }
}
