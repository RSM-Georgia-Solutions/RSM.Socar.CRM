using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Domain.Common;
using RSM.Socar.CRM.Domain.Identity;
using System.Linq.Expressions;
using System.Reflection;

namespace RSM.Socar.CRM.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Apply all IEntityTypeConfiguration<T> classes
        builder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        // 2. Apply soft delete global filters
        ApplySoftDeleteFilters(builder);

        // 3. Configure RowVersion for all BaseEntity types
        ConfigureBaseEntity(builder);
    }

    /// <summary>
    /// Applies global soft-delete filters for all entities implementing ISoftDeletable.
    /// </summary>
    private static void ApplySoftDeleteFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                // Build filter: e => !((ISoftDeletable)e).IsDeleted
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var prop = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var condition = Expression.Equal(prop, Expression.Constant(false));

                var lambda = Expression.Lambda(condition, parameter);
                entityType.SetQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Configures concurrency tokens (RowVersion) for all BaseEntity types.
    /// </summary>
    private static void ConfigureBaseEntity(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.RowVersion))
                    .IsRowVersion()
                    .IsConcurrencyToken();
            }
        }
    }
}
