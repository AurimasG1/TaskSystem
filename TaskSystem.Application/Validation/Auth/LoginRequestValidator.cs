using FluentValidation;
using TaskSystem.Application.DTO.Requests.Auth;

namespace TaskSystem.Application.Validation.Auth;

public class LoginRequestValidator : AbstractValidator<AuthLoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress();

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}
