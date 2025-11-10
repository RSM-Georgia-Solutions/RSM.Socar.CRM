using Microsoft.Extensions.DependencyInjection;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Web.Endpoints.Auth;

namespace RSM.Socar.CRM.Web.Extensions;

public static class WebApplicationExtensions
{
    // one-liner pipeline setup
    public static WebApplication UseWebPipeline(this WebApplication app)
        => app
            .UseSwaggerAndUi()
            .UseCorsAndAuth()
            .MapApiEndpoints();

    public static WebApplication UseSwaggerAndUi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }

    public static WebApplication UseCorsAndAuth(this WebApplication app)
    {
        app.UseCors("Default");
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapGet("/health", () => "OK");
        app.MapAuth(); // your existing extension that maps /api/auth/login
        return app;
    }

    // optional dev seeder
    public static async Task SeedDevDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await RSM.Socar.CRM.Infrastructure.Seed.DevUserSeeder.SeedAsync(db);
    }
}
