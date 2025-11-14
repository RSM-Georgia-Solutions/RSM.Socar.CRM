using MediatR;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.UserPermissions.Commands;

public abstract class RevokePermissionFromUserCommand
{
    public sealed record Request(int UserId, int PermissionId) : IRequest;

    public sealed class Handler(IUserRepository users, IUnitOfWork uow)
    : IRequestHandler<Request>
    {
        public async Task Handle(Request req, CancellationToken ct)
        {
            if (!await users.HasPermissionAsync(req.UserId, req.PermissionId, ct))
                throw new InvalidOperationException("User does not have this permission.");

            users.RemovePermission(new UserPermission
            {
                UserId = req.UserId,
                PermissionId = req.PermissionId
            });

            await uow.SaveChangesAsync(ct);
        }
    }
}
