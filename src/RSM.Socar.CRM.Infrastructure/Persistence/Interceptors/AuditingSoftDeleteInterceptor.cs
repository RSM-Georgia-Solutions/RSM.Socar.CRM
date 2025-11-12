using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Interceptors;

internal sealed class AuditingSoftDeleteInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> result)
    {
        if (e.Context is not DbContext db) return base.SavingChanges(e, result);

        var now = DateTime.UtcNow;
        var user = currentUser.UserId ?? currentUser.Email ?? currentUser.UserName;

        foreach (var entry in db.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
                SetCreateAudit(entry, now, user);

            if (entry.State == EntityState.Modified)
                SetModifyAudit(entry, now, user);

            if (entry.State == EntityState.Deleted)
            {
                // turn hard delete into soft delete
                if (entry.Entity is ISoftDeletable soft)
                {
                    entry.State = EntityState.Modified;
                    soft.IsDeleted = true;
                    soft.DeletedAtUtc = now;
                    soft.DeletedBy = user;
                    SetModifyAudit(entry, now, user);
                }
            }
        }

        return base.SavingChanges(e, result);
    }

    private static void SetCreateAudit(EntityEntry entry, DateTime now, string? user)
    {
        if (entry.Entity is IAuditable a)
        {
            a.CreatedAtUtc = a.CreatedAtUtc == default ? now : a.CreatedAtUtc; // don’t override seed
            a.CreatedBy ??= user;
            a.LastModifiedAtUtc = null;
            a.LastModifiedBy = null;
        }
        if (entry.Entity is ISoftDeletable s)
        {
            s.IsDeleted = s.IsDeleted && s.DeletedAtUtc != null; // keep seed choice if any
        }
    }

    private static void SetModifyAudit(EntityEntry entry, DateTime now, string? user)
    {
        if (entry.Entity is IAuditable a)
        {
            a.LastModifiedAtUtc = now;
            a.LastModifiedBy = user;
        }
    }
}
