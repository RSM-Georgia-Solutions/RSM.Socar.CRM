using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;

internal sealed class UserPermissionConfig : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> b)
    {
        b.ToTable("UserPermissions", Schemas.Identity);

        b.HasKey(x => new { x.UserId, x.PermissionId });

        b.HasOne(x => x.User)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.UserId);

        b.HasOne(x => x.Permission)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.PermissionId);
    }
}
