using Microsoft.EntityFrameworkCore.Storage;
using RSM.Socar.CRM.Application.Abstractions;

namespace RSM.Socar.CRM.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _currentTx;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        // If a transaction is already active, reuse it.
        _currentTx ??= await _db.Database.BeginTransactionAsync(ct);
        return _currentTx;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_currentTx is not null)
        {
            await _currentTx.CommitAsync(ct);
            await _currentTx.DisposeAsync();
            _currentTx = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_currentTx is not null)
        {
            await _currentTx.RollbackAsync(ct);
            await _currentTx.DisposeAsync();
            _currentTx = null;
        }
    }
}
