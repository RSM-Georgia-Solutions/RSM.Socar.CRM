using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RSM.Socar.CRM.Application.Abstractions;
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Users.Commands;

public abstract class ChangePasswordCommand
{
    public sealed record Request(
        int UserId,
        string OldPassword,
        string NewPassword,
        byte[] RowVersion
    ) : IRequest;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty();
            RuleFor(x => x.RowVersion).NotNull();
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

            // concurrency check
            users.MarkConcurrencyToken(user, req.RowVersion);

            // verify old password
            var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, req.OldPassword);
            if (verify == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Incorrect old password.");

            // update password
            user.PasswordHash = hasher.HashPassword(user, req.NewPassword);

            await uow.SaveChangesAsync(ct);
        }
    }
}
