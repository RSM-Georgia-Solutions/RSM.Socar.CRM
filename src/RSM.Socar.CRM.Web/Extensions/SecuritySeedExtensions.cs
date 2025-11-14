using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Infrastructure.Seed;

namespace RSM.Socar.CRM.Web.Extensions;

public static class SecuritySeedExtensions
{
    public static async Task SeedSecurityDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<RolePermissionSeeder>();

        await db.Database.MigrateAsync();
        await seeder.SeedAsync();
    }
}
