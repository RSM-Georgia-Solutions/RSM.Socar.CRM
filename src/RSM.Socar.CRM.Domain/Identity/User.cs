namespace RSM.Socar.CRM.Domain.Identity;

public sealed class User
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
}
