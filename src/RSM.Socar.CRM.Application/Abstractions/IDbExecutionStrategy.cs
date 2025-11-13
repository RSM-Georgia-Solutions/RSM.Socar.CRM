using Microsoft.EntityFrameworkCore.Storage;

namespace RSM.Socar.CRM.Application.Abstractions;

public interface IDbExecutionStrategy
{
    IExecutionStrategy CreateStrategy();
}
