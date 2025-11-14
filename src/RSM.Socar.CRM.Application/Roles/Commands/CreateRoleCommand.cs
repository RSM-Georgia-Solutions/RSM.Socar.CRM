using FluentValidation;
using MediatR;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Roles.Commands;

public abstract class CreateRoleCommand
{
    public sealed record Request(string Name, string? Description)
        : IRequest<Role>;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        }
    }

    public sealed class Handler(IRoleRepository roles, IUnitOfWork uow)
        : IRequestHandler<Request, Role>
    {
        public async Task<Role> Handle(Request req, CancellationToken ct)
        {
            if (await roles.ExistsByNameAsync(req.Name, ct))
                throw new InvalidOperationException("Role already exists.");

            var role = new Role
            {
                Name = req.Name,
                Description = req.Description
            };

            roles.Add(role);
            await uow.SaveChangesAsync(ct);

            return role;
        }
    }
}
