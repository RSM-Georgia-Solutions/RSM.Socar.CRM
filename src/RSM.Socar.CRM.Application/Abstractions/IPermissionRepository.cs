namespace RSM.Socar.CRM.Application.Abstractions;

using RSM.Socar.CRM.Domain.Identity;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(int id, CancellationToken ct);
}
