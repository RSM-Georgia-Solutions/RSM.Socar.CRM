using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Configurations;

internal abstract class BaseEntityConfig<T> : IEntityTypeConfiguration<T>
    where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> b)
    {
        // Primary Key
        b.HasKey(e => e.Id);

        // Concurrency / RowVersion
        b.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
