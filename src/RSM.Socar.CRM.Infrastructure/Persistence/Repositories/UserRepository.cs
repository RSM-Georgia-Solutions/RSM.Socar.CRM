using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.Users;
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


    // inside UserRepository (Infrastructure)
    public void MarkConcurrencyToken(User entity, byte[] rowVersion)
    {
        var entry = db.Entry(entity);
        entry.Property(u => u.RowVersion).OriginalValue = rowVersion;
    }
}
