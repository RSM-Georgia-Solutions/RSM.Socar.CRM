using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Repositories;

internal sealed class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _db;

    public PermissionRepository(AppDbContext db) => _db = db;

    public Task<Permission?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Permissions.FirstOrDefaultAsync(x => x.Id == id, ct);
}
