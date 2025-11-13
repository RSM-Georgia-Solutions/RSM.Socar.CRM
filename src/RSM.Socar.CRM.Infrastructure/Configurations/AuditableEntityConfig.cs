using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Configurations;

internal abstract class AuditableEntityConfig<T>
    : BaseEntityConfig<T>, IEntityTypeConfiguration<T>
    where T : AuditableEntity
{
    public override void Configure(EntityTypeBuilder<T> b)
    {
        base.Configure(b);

        // Auditing fields
        b.Property(e => e.CreatedAtUtc).HasColumnType("datetime2");
        b.Property(e => e.LastModifiedAtUtc).HasColumnType("datetime2");
        b.Property(e => e.CreatedBy).HasMaxLength(128);
        b.Property(e => e.LastModifiedBy).HasMaxLength(128);

        // Soft-delete fields
        b.Property(e => e.IsDeleted).HasDefaultValue(false);
        b.Property(e => e.DeletedAtUtc).HasColumnType("datetime2");
        b.Property(e => e.DeletedBy).HasMaxLength(128);
    }
}
