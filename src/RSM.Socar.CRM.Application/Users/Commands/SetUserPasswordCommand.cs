using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RSM.Socar.CRM.Application.Abstractions; // IUnitOfWork
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Users.Commands
{
    public abstract class SetUserPasswordCommand
    {
        public sealed record Request(int UserId, string Password, byte[] RowVersion) : IRequest;

        public sealed class Handler(
            IUserRepository users,
            IUnitOfWork uow,
            IPasswordHasher<User> hasher) : IRequestHandler<Request>
        {
            private readonly IUserRepository _users = users;
            private readonly IUnitOfWork _uow = uow;
            private readonly IPasswordHasher<User> _hasher = hasher;

            public async Task Handle(Request request, CancellationToken ct)
            {
                var user = await _users.GetByIdAsync(request.UserId, ct)
                    ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

                // 🔥 Critical: apply concurrency token
                _users.MarkConcurrencyToken(user, request.RowVersion);

                // Update password
                user.PasswordHash = _hasher.HashPassword(user, request.Password);

                await _uow.SaveChangesAsync(ct);
            }
        }



        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.UserId).GreaterThan(0);

                RuleFor(x => x.Password)
                    .NotEmpty()
                    .MinimumLength(8)
                    .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                    .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                    .Matches("[0-9]").WithMessage("Password must contain a digit.");
            }
        }
    }
}
