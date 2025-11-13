using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Users.Commands;

public abstract class ResetPasswordCommand
{
    public sealed record Request(int UserId, string NewPassword) : IRequest;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.NewPassword).NotEmpty();
        }
    }

    public sealed class Handler(
        IUserRepository users,
        IUnitOfWork uow,
        IPasswordHasher<User> hasher)
        : IRequestHandler<Request>
    {
        public async Task Handle(Request req, CancellationToken ct)
        {
            var user = await users.GetByIdAsync(req.UserId, ct)
                ?? throw new KeyNotFoundException($"User {req.UserId} not found.");

            // admin force override (no concurrency check)
            user.PasswordHash = hasher.HashPassword(user, req.NewPassword);

            await uow.SaveChangesAsync(ct);
        }
    }
}
