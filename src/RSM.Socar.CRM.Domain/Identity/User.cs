using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Domain.Identity;

public sealed class User : IAuditable, ISoftDeletable
{
    public int Id { get; set; }

    public string PersonalNo { get; set; } = default!; // 11 digits
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime? BirthDate { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

    // NEW: credentials
    public string PasswordHash { get; set; } = default!;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Auditing
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
