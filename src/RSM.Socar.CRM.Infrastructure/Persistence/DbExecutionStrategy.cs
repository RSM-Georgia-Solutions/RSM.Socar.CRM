using Microsoft.EntityFrameworkCore.Storage;
using RSM.Socar.CRM.Application.Abstractions;

namespace RSM.Socar.CRM.Infrastructure.Persistence;

internal sealed class DbExecutionStrategy(AppDbContext db) : IDbExecutionStrategy
{
    public IExecutionStrategy CreateStrategy() =>
        db.Database.CreateExecutionStrategy();
}
