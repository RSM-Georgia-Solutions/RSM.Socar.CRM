using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RSM.Socar.CRM.Web.Extensions;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing & metrics with sensible defaults.
    /// Reads OTLP endpoint from config/env if provided.
    /// Also enables Prometheus metrics exporter (expose via MapObservabilityEndpoints).
    /// </summary>
    public static IServiceCollection AddObservabilityLayer(this IServiceCollection services, IConfiguration cfg, string serviceName, string serviceVersion, string environmentName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", environmentName),
                    new KeyValuePair<string, object>("service.instance.id", Environment.MachineName),
                }))

            // ===== Tracing =====
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation(opt =>
                {
                    opt.RecordException = true;
                    opt.EnrichWithHttpRequest = (activity, req) =>
                    {
                        activity.SetTag("request.client_ip", req.HttpContext.Connection.RemoteIpAddress?.ToString());
                    };
                    opt.EnrichWithHttpResponse = (activity, res) =>
                    {
                        activity.SetTag("http.response_content_type", res.ContentType);
                    };
                })
                 .AddHttpClientInstrumentation(o => o.RecordException = true)
                 .AddSqlClientInstrumentation(o =>
                 {
                     o.RecordException = true;
                 })
                 .AddEntityFrameworkCoreInstrumentation();

                // Sampling (default 10%; override via config if desired)
                var sampleRate = cfg.GetValue<double?>("OpenTelemetry:TraceSamplingRate") ?? 0.10;
                t.SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(sampleRate)));
                t.AddOtlpExporter();                   // ensure traces go to dashboard

                // resolve endpoint
                var otlpEndpoint =
                    Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ??
                    cfg.GetValue<string?>("OpenTelemetry:Otlp:Endpoint");

                t.AddOtlpExporter(o =>
                {
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        o.Endpoint = new Uri(otlpEndpoint);

                    // protocol
                    var protocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL")
                                ?? "grpc";
                    o.Protocol = protocol.Equals("http", StringComparison.OrdinalIgnoreCase)
                        ? OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf
                        : OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;

                    // headers
                    var headers = cfg.GetValue<string?>("OpenTelemetry:Otlp:Headers");
                    if (!string.IsNullOrWhiteSpace(headers))
                        o.Headers = headers;
                });

            })

            // ===== Metrics =====
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation()
                 .AddRuntimeInstrumentation()
                 .AddProcessInstrumentation()
                 .AddPrometheusExporter();   // (optional) keep /metrics for Prometheus
            });

        return services;
    }

    /// <summary>
    /// Maps aux endpoints for observability (e.g., Prometheus /metrics).
    /// Call this once after app.Build().
    /// </summary>
    public static WebApplication MapObservabilityEndpoints(this WebApplication app, string metricsPath = "/metrics")
    {
        // Prometheus scrape endpoint
        app.MapPrometheusScrapingEndpoint(metricsPath);
        return app;
    }
}
