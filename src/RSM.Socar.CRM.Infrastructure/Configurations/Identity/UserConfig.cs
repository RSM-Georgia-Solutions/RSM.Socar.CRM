using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Configurations.Identity;

internal sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users", Schemas.Identity);
        b.HasKey(x => x.Id);

        b.Property(x => x.PersonalNo).HasMaxLength(11);
        b.HasIndex(x => x.PersonalNo).IsUnique();

        b.Property(x => x.Email).HasMaxLength(256);
        b.HasIndex(x => x.Email);

        b.Property(x => x.Mobile).HasMaxLength(16);
        b.Property(x => x.FirstName).HasMaxLength(128);
        b.Property(x => x.LastName).HasMaxLength(128);

        b.Property(x => x.PasswordHash).IsRequired();

        // Concurrency token
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        // Auditing columns
        b.Property(x => x.CreatedAtUtc).HasColumnType("datetime2");
        b.Property(x => x.LastModifiedAtUtc).HasColumnType("datetime2");
        b.Property(x => x.CreatedBy).HasMaxLength(128);
        b.Property(x => x.LastModifiedBy).HasMaxLength(128);

        // Soft delete columns
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedAtUtc).HasColumnType("datetime2");
        b.Property(x => x.DeletedBy).HasMaxLength(128);

        // Global query filter for soft delete
        b.HasQueryFilter(u => !u.IsDeleted);
    }
}
