using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Application.Security;

namespace RSM.Socar.CRM.Infrastructure.Security
{
    public sealed class PermissionEvaluator : IPermissionEvaluator
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _users;

        private HashSet<string>? _cached;

        public PermissionEvaluator(ICurrentUser currentUser, IUserRepository users)
        {
            _currentUser = currentUser;
            _users = users;
        }

        public async Task<bool> HasPermissionAsync(string permission, CancellationToken ct)
        {
            // No user authenticated
            if (string.IsNullOrWhiteSpace(_currentUser.UserId))
                return false;

            // Convert string → int
            if (!int.TryParse(_currentUser.UserId, out var userId))
                return false;

            // Cache permissions for this request
            if (_cached is null)
            {
                var user = await _users.GetByIdWithRolesAsync(userId, ct);

                if (user is null)
                    return false;

                _cached = new HashSet<string>(
                    user.Roles
                        .SelectMany(r => r.Role.RolePermissions.Select(p => p.Permission.Name)),
                    StringComparer.OrdinalIgnoreCase
                );
            }

            return _cached.Contains(permission);
        }
    }

}
