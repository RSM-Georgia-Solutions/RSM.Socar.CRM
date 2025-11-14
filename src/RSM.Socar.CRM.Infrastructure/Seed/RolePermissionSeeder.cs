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
        // ---------------------------------------------------------
        // 1) Discover all permission names (from tables, columns, attributes)
        // ---------------------------------------------------------
        var discoveredNames = await _scanner.DiscoverAsync();

        // Load existing permissions
        var existing = await _db.Permissions.AsNoTracking().ToListAsync();
        var existingNames = existing.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // ---------------------------------------------------------
        // 2) Insert missing permissions
        // ---------------------------------------------------------
        foreach (var name in discoveredNames)
        {
            if (!existingNames.Contains(name))
            {
                _db.Permissions.Add(new Permission
                {
                    Name = name,
                    Description = null
                });
            }
        }

        await _db.SaveChangesAsync();

        // Reload permissions after insert
        var allPermissions = await _db.Permissions.ToListAsync();

        // ---------------------------------------------------------
        // 3) Ensure Admin role exists
        // ---------------------------------------------------------
        var admin = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == "Admin");

        if (admin is null)
        {
            admin = new Role
            {
                Name = "Admin",
                Description = "System Administrator"
            };

            _db.Roles.Add(admin);
            await _db.SaveChangesAsync();
        }

        // Admin existing permissions
        var adminPermissionIds = admin.RolePermissions
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        // ---------------------------------------------------------
        // 4) Give Admin ALL permissions
        // ---------------------------------------------------------
        foreach (var p in allPermissions)
        {
            if (!adminPermissionIds.Contains(p.Id))
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
