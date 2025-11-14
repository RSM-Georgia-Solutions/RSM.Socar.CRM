using MediatR;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.UserPermissions.Commands;

public abstract class GrantPermissionToUserCommand
{
    public sealed record Request(int UserId, int PermissionId) : IRequest;

    public sealed class Handler(IUserRepository users, IPermissionRepository perms, IUnitOfWork uow)
    : IRequestHandler<Request>
    {
        public async Task Handle(Request req, CancellationToken ct)
        {
            var user = await users.GetByIdAsync(req.UserId, ct)
                ?? throw new KeyNotFoundException("User not found");

            var permission = await perms.GetByIdAsync(req.PermissionId, ct)
                ?? throw new KeyNotFoundException("Permission not found");

            if (await users.HasPermissionAsync(req.UserId, req.PermissionId, ct))
                throw new InvalidOperationException("User already has this permission.");

            users.AddPermission(new UserPermission
            {
                UserId = req.UserId,
                PermissionId = req.PermissionId
            });

            await uow.SaveChangesAsync(ct);
        }
    }
}
