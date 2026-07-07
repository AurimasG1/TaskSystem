using FluentValidation;
using TaskSystem.Application.Commands.Auth.AuthLogin;

public class LoginUserCommandValidator : AbstractValidator<AuthLoginCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Password).NotEmpty();
    }
}
