using FluentValidation;
using TaskSystem.Application.DTO;

namespace TaskSystem.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress();

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}
