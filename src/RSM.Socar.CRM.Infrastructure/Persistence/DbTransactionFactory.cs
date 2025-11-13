using Microsoft.EntityFrameworkCore.Storage;

namespace RSM.Socar.CRM.Infrastructure.Persistence;

internal sealed class DbTransactionFactory(AppDbContext db) : IDbTransactionFactory
{
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        db.Database.BeginTransactionAsync(ct);
}
