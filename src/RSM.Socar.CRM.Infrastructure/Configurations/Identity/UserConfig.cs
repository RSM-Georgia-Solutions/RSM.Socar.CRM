using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;

internal sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users", Schemas.Identity);
        b.HasKey(x => x.Id);

        // Requireds + lengths
        b.Property(x => x.PersonalNo).HasMaxLength(11).IsRequired();
        b.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(128).IsRequired();
        b.Property(x => x.Email).HasMaxLength(256);
        b.Property(x => x.Mobile).HasMaxLength(16);
        b.Property(x => x.Position).HasMaxLength(128);

        // Uniqueness
        b.HasIndex(x => x.PersonalNo).IsUnique();
        // Email is optional: make it unique only when not null
        b.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

        // Audit/defaults
        b.Property(x => x.RegisteredAtUtc)
         .HasColumnType("datetime2")
         .HasDefaultValueSql("SYSUTCDATETIME()");

        // Security
        b.Property(x => x.PasswordHash).IsRequired();

        // Concurrency (ETag/rowversion)
        b.Property(x => x.RowVersion)
         .IsRowVersion()
         .IsConcurrencyToken();
    }
}
