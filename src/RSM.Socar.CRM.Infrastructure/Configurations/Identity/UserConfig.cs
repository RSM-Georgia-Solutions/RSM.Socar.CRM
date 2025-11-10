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
    }
}
