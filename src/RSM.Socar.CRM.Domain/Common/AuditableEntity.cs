namespace RSM.Socar.CRM.Domain.Common;

/// <summary>
/// Entity base class that includes auditing information and soft-delete support.
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditable, ISoftDeletable
{
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
