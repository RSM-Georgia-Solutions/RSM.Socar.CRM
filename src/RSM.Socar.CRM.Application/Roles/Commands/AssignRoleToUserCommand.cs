using MediatR;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.UserRoles.Commands;

public abstract class AssignRoleToUserCommand
{
    public sealed record Request(int UserId, int RoleId) : IRequest;

    public sealed class Handler(IUserRepository users, IRoleRepository roles, IUnitOfWork uow)
    : IRequestHandler<Request>
    {
        public async Task Handle(Request req, CancellationToken ct)
        {
            var user = await users.GetByIdAsync(req.UserId, ct)
                ?? throw new KeyNotFoundException("User not found");

            var role = await roles.GetByIdAsync(req.RoleId, ct)
                ?? throw new KeyNotFoundException("Role not found");

            if (await users.HasRoleAsync(req.UserId, req.RoleId, ct))
                throw new InvalidOperationException("User already has this role.");

            users.AddRole(new UserRole
            {
                UserId = req.UserId,
                RoleId = req.RoleId
            });

            await uow.SaveChangesAsync(ct);
        }
    }

}
