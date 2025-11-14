namespace RSM.Socar.CRM.Application.Abstractions;

using RSM.Socar.CRM.Domain.Identity;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);

    void Add(Role role);
    void Remove(Role role);
}
