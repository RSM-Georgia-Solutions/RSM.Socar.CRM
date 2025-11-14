using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Infrastructure.Security;

namespace RSM.Socar.CRM.Infrastructure.Seed;

public sealed class RolePermissionSeeder
{
    private readonly AppDbContext _db;
    private readonly IPermissionDiscoveryService _scanner;

    public RolePermissionSeeder(AppDbContext db, IPermissionDiscoveryService scanner)
    {
        _db = db;
        _scanner = scanner;
    }

    public async Task SeedAsync()
    {
        var discovered = await _scanner.DiscoverAsync();
        var existing = await _db.Permissions.AsNoTracking().ToListAsync();

        // Insert missing
        foreach (var p in discovered)
        {
            if (existing.All(x => x.Name != p.Name))
                _db.Permissions.Add(p);
        }

        await _db.SaveChangesAsync();

        // Admin Role
        var admin = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == "Admin");

        if (admin is null)
        {
            admin = new Role { Name = "Admin", Description = "System Administrator" };
            _db.Roles.Add(admin);
            await _db.SaveChangesAsync();
        }

        // Assign all permissions
        var allPermissions = await _db.Permissions.ToListAsync();

        foreach (var p in allPermissions)
        {
            if (admin.RolePermissions.All(rp => rp.PermissionId != p.Id))
            {
                admin.RolePermissions.Add(new RolePermission
                {
                    RoleId = admin.Id,
                    PermissionId = p.Id
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}
