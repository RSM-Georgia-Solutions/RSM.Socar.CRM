namespace RSM.Socar.CRM.Domain.Common;

/// <summary>
/// Base class for all entities providing identity and concurrency control.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key identity, integer-based.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Optimistic concurrency token used by EF Core.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
