using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> GetByIdNoTrackingAsync(int id, CancellationToken ct) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);


    public Task<bool> PersonalNoExistsAsync(string personalNo, int? excludeId, CancellationToken ct) =>
    db.Users.AsNoTracking().AnyAsync(u => u.PersonalNo == personalNo && (excludeId == null || u.Id != excludeId), ct);

    public Task<bool> EmailExistsAsync(string email, int? excludeId, CancellationToken ct) =>
        db.Users.AsNoTracking().AnyAsync(u => u.Email == email && (excludeId == null || u.Id != excludeId), ct);


    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);


    public void Add(User user) => db.Users.Add(user);


    public void Remove(User user) => db.Users.Remove(user);


    public async Task<User?> GetByIdWithRolesAsync(int id, CancellationToken ct)
    {
        return await db.Users
            .AsNoTracking()
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }


    public void AddRole(UserRole entity) => db.UserRoles.Add(entity);

    public void RemoveRole(UserRole entity) => db.UserRoles.Remove(entity);

    // -------------------------
    // NEW Permission helpers
    // -------------------------


    public async Task<bool> HasRoleAsync(int userId, int roleId, CancellationToken ct)
       => await db.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId, ct);


    // inside UserRepository (Infrastructure)
    public void MarkConcurrencyToken(User entity, byte[] rowVersion)
    {
        var entry = db.Entry(entity);
        entry.Property(u => u.RowVersion).OriginalValue = rowVersion;
    }

    public Task<User?> GetByPersonalNoAsync(string personalNo, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.PersonalNo == personalNo, ct);
}
