using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RSM.Socar.CRM.Web.Endpoints.Auth;

namespace RSM.Socar.CRM.Web.Extensions;

public static class EndpointMappings
{
    // Usage: app.MapApiEndpoints();
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        // API
        app.MapControllers();
        app.MapAuth(); // your existing extension that maps /api/auth/login

        // ---- Platform endpoints ----
        app.MapObservabilityEndpoints("/metrics");   // Prometheus scrape

        // Health (liveness & readiness)
        app.MapHealthChecks("/_health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
            ResponseWriter = WriteHealthJson
        });

        app.MapHealthChecks("/_health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            ResponseWriter = WriteHealthJson
        });

        // Simple OK for humans/load balancers
        app.MapGet("/health", () => Results.Ok("OK"));

        // Service info
        app.MapGet("/_info", () =>
        {
            var asm = Assembly.GetEntryAssembly()?.GetName();
            return Results.Json(new
            {
                name = asm?.Name ?? "RSM.Socar.CRM",
                version = asm?.Version?.ToString(),
                environment = app.Environment.EnvironmentName
            });
        });

        // Root → Swagger
        app.MapGet("/", ctx =>
        {
            ctx.Response.Redirect("/swagger", permanent: false);
            return Task.CompletedTask;
        });

        return app;
    }

    private static Task WriteHealthJson(HttpContext ctx, HealthReport report)
    {
        ctx.Response.ContentType = "application/json; charset=utf-8";
        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                tags = e.Value.Tags,
                data = e.Value.Data
            })
        };
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
