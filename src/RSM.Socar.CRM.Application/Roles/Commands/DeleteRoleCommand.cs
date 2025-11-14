using MediatR;
using RSM.Socar.CRM.Application.Abstractions;

namespace RSM.Socar.CRM.Application.Roles.Commands;

public abstract class DeleteRoleCommand
{
    public sealed record Request(int Id) : IRequest;

    public sealed class Handler(IRoleRepository roles, IUnitOfWork uow)
        : IRequestHandler<Request>
    {
        public async Task Handle(Request req, CancellationToken ct)
        {
            var role = await roles.GetByIdAsync(req.Id, ct)
                ?? throw new KeyNotFoundException("Role not found");

            roles.Remove(role);
            await uow.SaveChangesAsync(ct);
        }
    }
}
