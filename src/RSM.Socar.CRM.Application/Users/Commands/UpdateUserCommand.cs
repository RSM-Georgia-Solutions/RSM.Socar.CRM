using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RSM.Socar.CRM.Application.Abstractions;

namespace RSM.Socar.CRM.Application.Users.Commands;


public abstract class UpdateUserCommand
{
    public sealed record Request(
    int Id,
    string PersonalNo,
    string FirstName,
    string LastName,
    DateTime? BirthDate,
    string? Mobile,
    string? Email,
    string? Position,
    bool IsActive,
    byte[] RowVersion // for concurrency
) : IRequest;

    // ---------- Validator ----------
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);

            RuleFor(x => x.PersonalNo)
                .NotEmpty().WithMessage("PersonalNo is required.")
                .Length(11).WithMessage("PersonalNo must be 11 digits.");

            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.RowVersion)
                .NotNull().WithMessage("RowVersion is required for concurrency.")
                .Must(rv => rv.Length > 0).WithMessage("RowVersion cannot be empty.");
        }
    }

    // ---------- Handler ----------
    public sealed class Handler(IUserRepository users, IUnitOfWork uom) : IRequestHandler<Request>
    {
        private readonly IUserRepository _users = users;
        private readonly IUnitOfWork _uow = uom;

        public async Task Handle(Request cmd, CancellationToken ct)
        {
            var entity = await _users.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                throw new KeyNotFoundException($"User {cmd.Id} not found.");

            // Optimistic concurrency: set original RowVersion to the client’s value
            // (implemented in Infrastructure to avoid EF references here)
            _users.MarkConcurrencyToken(entity, cmd.RowVersion);

            // Optional uniqueness checks (exclude current Id)
            if (!string.IsNullOrWhiteSpace(cmd.PersonalNo) &&
                await _users.PersonalNoExistsAsync(cmd.PersonalNo, excludeId: cmd.Id, ct))
            {
                throw new InvalidOperationException("PersonalNo already exists.");
            }

            if (!string.IsNullOrWhiteSpace(cmd.Email) &&
                await _users.EmailExistsAsync(cmd.Email, excludeId: cmd.Id, ct))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            // Apply allowed fields (never touch PasswordHash here)
            entity.PersonalNo = cmd.PersonalNo;
            entity.FirstName = cmd.FirstName;
            entity.LastName = cmd.LastName;
            entity.BirthDate = cmd.BirthDate;
            entity.Mobile = cmd.Mobile;
            entity.Email = cmd.Email;
            entity.Position = cmd.Position;
            entity.IsActive = cmd.IsActive;

            await _uow.SaveChangesAsync(ct);
        }
    }

}