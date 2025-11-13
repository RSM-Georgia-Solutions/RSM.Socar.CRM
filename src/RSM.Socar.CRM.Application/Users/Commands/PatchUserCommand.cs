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

            _users.ApplyDelta(entity, cmd.Patch);
            await _uow.SaveChangesAsync(ct);
        }
    }
}