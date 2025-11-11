using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.Extensions.DependencyInjection;
using RSM.Socar.CRM.Application.Abstractions; // IUnitOfWork
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Users.Commands;


public abstract class PatchUserCommand
{
    // Flat command record
    public sealed record Request(int Id, Delta<User> Patch) : IRequest;

    // Validator
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Patch).NotNull();
        }
    }

    // Handler
    public sealed class Handler : IRequestHandler<Request>
    {
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _uow;

        public Handler(IServiceProvider services)
        {
            _users = services.GetRequiredService<IUserRepository>();
            _uow = services.GetRequiredService<IUnitOfWork>();
        }

        public async Task Handle(Request cmd, CancellationToken ct)
        {
            var entity = await _users.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                throw new KeyNotFoundException($"User {cmd.Id} not found.");

            // Disallow changing sensitive fields via PATCH
            cmd.Patch.TryGetPropertyValue(nameof(User.PasswordHash), out _);
            cmd.Patch.TrySetPropertyValue(nameof(User.PasswordHash), entity.PasswordHash);

            // Optional: whitelist fields (example)
            // var changed = cmd.Patch.GetChangedPropertyNames();
            // if (changed.Any(n => n is nameof(User.RowVersion) or nameof(User.Id)))
            //     throw new InvalidOperationException("Cannot modify Id/RowVersion via PATCH.");

            cmd.Patch.Patch(entity);
            await _uow.SaveChangesAsync(ct);
        }
    }
}