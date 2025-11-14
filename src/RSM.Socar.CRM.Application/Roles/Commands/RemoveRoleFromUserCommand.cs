using MediatR;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.UserRoles.Commands;

public abstract class RemoveRoleFromUserCommand
{
    public sealed record Request(int UserId, int RoleId) : IRequest;

    public sealed class Handler(IUserRepository users, IUnitOfWork uow)
    : IRequestHandler<Request>
    {
        public async Task Handle(Request req, CancellationToken ct)
        {
            if (!await users.HasRoleAsync(req.UserId, req.RoleId, ct))
                throw new InvalidOperationException("User does not have this role.");

            users.RemoveRole(new UserRole
            {
                UserId = req.UserId,
                RoleId = req.RoleId
            });

            await uow.SaveChangesAsync(ct);
        }
    }
}
