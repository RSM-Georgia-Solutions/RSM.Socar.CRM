using Microsoft.EntityFrameworkCore.Storage;

public interface IDbTransactionFactory
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
