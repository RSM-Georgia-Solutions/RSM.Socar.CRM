using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Domain.Identity;

public sealed class User : AuditableEntity
{
    public string PersonalNo { get; set; } = default!; // 11 digits
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime? BirthDate { get; set; }

    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// First registration date of the user.
    /// This is not overridden by audit interceptor.
    /// </summary>
    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Hashed password. Always required.
    /// </summary>
    public string PasswordHash { get; set; } = default!;

    public ICollection<UserRole> Roles { get; set; } = [];
    public ICollection<UserPermission> Permissions { get; set; } = [];
}
