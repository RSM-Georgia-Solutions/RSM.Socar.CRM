using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Interceptors;

internal sealed class AuditingSoftDeleteInterceptor(ICurrentUser currentUser)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not DbContext db)
            return base.SavingChanges(eventData, result);

        ApplyAuditInformation(db);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is DbContext db)
            ApplyAuditInformation(db);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditInformation(DbContext db)
    {
        string? user = currentUser.UserId ?? currentUser.Email ?? currentUser.UserName;
        DateTime now = DateTime.UtcNow;

        foreach (var entry in db.ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseEntity) // skip non-entities
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    ApplyCreateAudit(entry, now, user);
                    break;

                case EntityState.Modified:
                    ApplyModifyAudit(entry, now, user);
                    break;

                case EntityState.Deleted:
                    ApplySoftDelete(entry, now, user);
                    break;
            }
        }
    }

    private void ApplyCreateAudit(EntityEntry entry, DateTime now, string? user)
    {
        if (entry.Entity is IAuditable a)
        {
            // Only set CreatedAt for new entities (not for seeded data)
            if (a.CreatedAtUtc == default)
                a.CreatedAtUtc = now;

            a.CreatedBy ??= user;

            // New entities should not have modification metadata
            a.LastModifiedAtUtc = null;
            a.LastModifiedBy = null;
        }

        if (entry.Entity is ISoftDeletable soft)
        {
            // Leave IsDeleted untouched if seeded as deleted
            if (!soft.IsDeleted)
            {
                soft.DeletedAtUtc = null;
                soft.DeletedBy = null;
            }
        }
    }

    private void ApplyModifyAudit(EntityEntry entry, DateTime now, string? user)
    {
        if (entry.Entity is IAuditable a)
        {
            a.LastModifiedAtUtc = now;
            a.LastModifiedBy = user;
        }
    }

    private void ApplySoftDelete(EntityEntry entry, DateTime now, string? user)
    {
        if (entry.Entity is not ISoftDeletable soft)
            return;

        // Convert hard delete → soft delete
        entry.State = EntityState.Modified;

        soft.IsDeleted = true;
        soft.DeletedAtUtc = now;
        soft.DeletedBy = user;

        // Also update LastModified metadata if applicable
        if (entry.Entity is IAuditable a)
        {
            a.LastModifiedAtUtc = now;
            a.LastModifiedBy = user;
        }
    }
}
