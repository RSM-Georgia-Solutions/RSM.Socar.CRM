using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> PersonalNoExistsAsync(string personalNo, int? excludeId, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, int? excludeId, CancellationToken ct);
    // Applies the original concurrency token in the persistence layer
    void MarkConcurrencyToken(User entity, byte[] rowVersion);
    void Add(User user);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetByPersonalNoAsync(string personalNo, CancellationToken ct);
    void Remove(User entity);
    Task<User?> GetByIdWithRolesAsync(int id, CancellationToken ct);

    void AddRole(UserRole entity);
    void RemoveRole(UserRole entity);

    Task<bool> HasRoleAsync(int userId, int roleId, CancellationToken ct);
}
