using Serilog;
using Serilog.Enrichers.Span;
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
            var otlp = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                      ?? "http://localhost:4317"; // fallback for local runs

            cfg.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithMachineName()
              .Enrich.WithEnvironmentName()
              .Enrich.WithProcessId()
              .Enrich.WithThreadId()
              .Enrich.WithExceptionDetails()
              .Enrich.WithSpan() // TraceId / SpanId
              .WriteTo.Console()
              .WriteTo.OpenTelemetry(options =>
              {
                  options.Endpoint = otlp; // <-- string, not Uri
                  options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
                  options.ResourceAttributes = new Dictionary<string, object?>
                  {
                      ["service.name"] = "RSM.Socar.CRM.Web",
                      ["deployment.environment"] = ctx.HostingEnvironment.EnvironmentName
                  };
              });
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
