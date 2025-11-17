using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Application.Security;
using RSM.Socar.CRM.Domain.Common;

namespace RSM.Socar.CRM.Infrastructure.Persistence.Interceptors;

public sealed class AuthorizationInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionEvaluator _permissions;
    private readonly IHttpContextAccessor _http;

    public AuthorizationInterceptor(
        ICurrentUser currentUser,
        IPermissionEvaluator permissions,
        IHttpContextAccessor http)
    {
        _currentUser = currentUser;
        _permissions = permissions;
        _http = http;
    }

    private bool IsHttpRequest =>
        _http.HttpContext is not null;

    private bool IsAuthenticated =>
        _currentUser.UserId is not null;

    private bool IsRunningMigrations(DbContext db) =>
        db.GetService<IDesignTimeServices>() != null; // Always false runtime, true in migrations

    private bool IsSeeder(DbContext db) =>
        db.GetType().Name.Contains("Seeder", StringComparison.OrdinalIgnoreCase);

    private bool IsSwagger =>
        _http.HttpContext?.Request.Path.Value?.Contains("swagger", StringComparison.OrdinalIgnoreCase) == true;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        await SafeCheckPermissionsAsync(eventData, ct);
        return result;
    }

    private async Task SafeCheckPermissionsAsync(DbContextEventData eventData, CancellationToken ct)
    {
        if (eventData.Context is not DbContext db)
            return;

        // ---- STOP CONDITIONS ----

        // No HTTP request? (migrations, background, seeding)
        if (!IsHttpRequest)
            return;

        // Swagger calls (model binding changes entries)
        if (IsSwagger)
            return;

        // Seeder or migrations
        if (!IsAuthenticated)
            return;

        // ---- REAL SECURITY STARTS HERE ----

        foreach (var entry in db.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            if (entry.Entity is not BaseEntity)
                continue;

            var entityName = entry.Entity.GetType().Name;
            var required = new HashSet<string>();

            switch (entry.State)
            {
                case EntityState.Added:
                    required.Add($"{entityName}.Create");
                    break;

                case EntityState.Modified:
                    required.Add($"{entityName}.Update");
                    break;

                case EntityState.Deleted:
                    required.Add($"{entityName}.Delete");
                    break;
            }

            foreach (var perm in required)
            {
                if (!await _permissions.HasPermissionAsync(perm, ct))
                    throw new UnauthorizedAccessException($"Missing permission: {perm}");
            }
        }
    }
}
