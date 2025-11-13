using FluentValidation;
using MediatR;
using RSM.Socar.CRM.Application.Abstractions;

namespace RSM.Socar.CRM.Application.Users.Commands;

public static class DeleteUserCommand
{
    // ------------------------------
    // REQUEST
    // ------------------------------
    public sealed record Request(int Id) : IRequest;

    // ------------------------------
    // VALIDATOR
    // ------------------------------
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    // ------------------------------
    // HANDLER
    // ------------------------------
    public sealed class Handler(
        IUserRepository users,
        IUnitOfWork uow)
        : IRequestHandler<Request>
    {
        private readonly IUserRepository _users = users;
        private readonly IUnitOfWork _uow = uow;

        public async Task Handle(Request request, CancellationToken ct)
        {
            var entity = await _users.GetByIdAsync(request.Id, ct);

            if (entity is null)
                throw new KeyNotFoundException($"User {request.Id} not found.");

            // EF soft delete interceptor will convert this into IsDeleted = true automatically
            _users.Remove(entity);

            await _uow.SaveChangesAsync(ct);
        }
    }
}
