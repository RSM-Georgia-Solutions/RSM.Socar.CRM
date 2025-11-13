using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> PersonalNoExistsAsync(string personalNo, int? excludeId, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, int? excludeId, CancellationToken ct);
    // Applies the original concurrency token in the persistence layer
    void MarkConcurrencyToken(User entity, byte[] rowVersion);
    void Add(User user);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    void Remove(User entity);
}
