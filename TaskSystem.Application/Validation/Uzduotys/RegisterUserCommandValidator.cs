using FluentValidation;
using TaskSystem.Application.Commands.Users.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

        RuleFor(x => x.AdminCode)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.AdminCode));
    }
}
