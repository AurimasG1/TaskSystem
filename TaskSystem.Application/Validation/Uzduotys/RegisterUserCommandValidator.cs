using FluentValidation;
using TaskSystem.Application.Commands.Auth.AuthRegister;

public class RegisterUserCommandValidator : AbstractValidator<AuthRegisterCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

        // RuleFor(x => x.AdminCode)
        //     .MaximumLength(100)
        //     .When(x => !string.IsNullOrWhiteSpace(x.AdminCode));
    }
}
