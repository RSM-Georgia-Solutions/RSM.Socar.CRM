using Microsoft.AspNetCore.Identity;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;

namespace RSM.Socar.CRM.Infrastructure.Seed;

public static class DevUserSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (db.Users.Any()) return;

        var user = new User
        {
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            PersonalNo = "12345678901",
            IsActive = true,
            RegisteredAtUtc = DateTime.UtcNow
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, "Passw0rd!");

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }
}
