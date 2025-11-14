using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;

    public RoleRepository(AppDbContext db) => _db = db;

    public Task<Role?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Roles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct) =>
        _db.Roles.AsNoTracking().AnyAsync(x => x.Name == name, ct);

    public void Add(Role role) =>
        _db.Roles.Add(role);

    public void Remove(Role role) =>
        _db.Roles.Remove(role);
}
