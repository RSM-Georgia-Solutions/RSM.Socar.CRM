using FluentValidation;
using MediatR;
using RSM.Socar.CRM.Application.Abstractions;

namespace RSM.Socar.CRM.Application.Roles.Commands;

public abstract class UpdateRoleCommand
{
    public sealed record Request(int Id, string Name, string? Description)
        : IRequest;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public sealed class Handler(IRoleRepository roles, IUnitOfWork uow)
        : IRequestHandler<Request>
    {
        public async Task Handle(Request req, CancellationToken ct)
        {
            var role = await roles.GetByIdAsync(req.Id, ct)
                ?? throw new KeyNotFoundException("Role not found");

            role.Name = req.Name;
            role.Description = req.Description;

            await uow.SaveChangesAsync(ct);
        }
    }
}
