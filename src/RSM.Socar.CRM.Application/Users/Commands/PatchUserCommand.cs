using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.OData.Deltas;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Users.Commands;

public abstract class PatchUserCommand
{
    public sealed record Request(int Id, Delta<User> Patch, byte[] RowVersion) : IRequest;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Patch).NotNull();
            RuleFor(x => x.RowVersion).NotNull();
        }
    }

    public sealed class Handler(IUserRepository users, IUnitOfWork uow)
        : IRequestHandler<Request>
    {
        private readonly IUserRepository _users = users;
        private readonly IUnitOfWork _uow = uow;

        public async Task Handle(Request cmd, CancellationToken ct)
        {
            var entity = await _users.GetByIdAsync(cmd.Id, ct)
                ?? throw new KeyNotFoundException($"User {cmd.Id} not found.");

            // 🔥 Apply concurrency token (CRITICAL)
            _users.MarkConcurrencyToken(entity, cmd.RowVersion);

            // Apply OData delta
            cmd.Patch.Patch(entity);

            await _uow.SaveChangesAsync(ct);
        }
    }
}
