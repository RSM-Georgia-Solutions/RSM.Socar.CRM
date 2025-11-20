using FluentValidation;
using MediatR;
using RSM.Socar.CRM.Application.Abstractions; // IUnitOfWork
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Users.Commands
{
    // Depends only on abstractions; no EF/Core here.
    public abstract class CreateUserCommand
    {
        // You wanted the full entity back; that's fine for now.
        public sealed record Request(
            string PersonalNo,
            string FirstName,
            string LastName,
            DateTime? BirthDate,
            string? Mobile,
            string? Email,
            string? Position
        ) : IRequest<User>;

        public sealed class Handler(IUserRepository users, IUnitOfWork uom) : IRequestHandler<Request, User>
        {
            private readonly IUserRepository _users = users;
            private readonly IUnitOfWork _uow = uom;

            public async Task<User> Handle(Request req, CancellationToken ct)
            {
                // Uniqueness checks via repository (no EF in Application)
                if (await _users.PersonalNoExistsAsync(req.PersonalNo, excludeId: null, ct))
                    throw new InvalidOperationException("PersonalNo already exists.");

                if (!string.IsNullOrWhiteSpace(req.Email) &&
                    await _users.EmailExistsAsync(req.Email, excludeId: null, ct))
                    throw new InvalidOperationException("Email already exists.");

                var u = new User
                {
                    Id = 0,
                    PersonalNo = req.PersonalNo,
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    BirthDate = req.BirthDate,
                    Mobile = req.Mobile,
                    Email = req.Email,
                    Position = req.Position,
                    Status = Domain.Enums.UserStatus.Active,
                    RegisteredAtUtc = DateTime.UtcNow,
                    PasswordHash = string.Empty // set later via SetPassword flow
                };

                _users.Add(u);
                await _uow.SaveChangesAsync(ct);
                return u;
            }
        }

        public sealed class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.PersonalNo)
                    .NotEmpty().WithMessage("PersonalNo is required.")
                    .Length(11).WithMessage("PersonalNo must be 11 digits.");

                RuleFor(x => x.FirstName).NotEmpty();
                RuleFor(x => x.LastName).NotEmpty();

                RuleFor(x => x.Email)
                    .EmailAddress()
                    .When(x => !string.IsNullOrWhiteSpace(x.Email));
            }
        }
    }
}
