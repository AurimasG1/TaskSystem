using FluentValidation;
using TaskSystem.Application.DTO;

namespace TaskSystem.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);

        // RuleFor(x => x.Password)
        //     .NotEmpty()
        //     .MinimumLength(10)
        //     .Matches("[A-Z]")
        //     .WithMessage("Password must contain at least one uppercase letter")
        //     .Matches("[a-z]")
        //     .WithMessage("Password must contain at least one lowercase letter")
        //     .Matches("[0-9]")
        //     .WithMessage("Password must contain at least one number")
        //     .Matches("[^a-zA-Z0-9]")
        //     .WithMessage("Password must contain at least one special character")
        //     .Must(p => !p.ToLower().Contains("password"))
        //     .WithMessage("Password cannot contain the word 'password'")
        //     .Must(p => !p.Contains("123"))
        //     .WithMessage("Password cannot contain sequences like 123")
        //     .Must(p => !p.Contains("qwerty"))
        //     .WithMessage("Password cannot contain common patterns like qwerty");

        // RuleFor(x => x.Password)
        //     .NotEmpty()
        //     .MinimumLength(8)
        //     .Matches("[A-Z]")
        //     .WithMessage("Password must contain at least one uppercase letter")
        //     .Matches("[a-z]")
        //     .WithMessage("Password must contain at least one lowercase letter")
        //     .Matches("[0-9]")
        //     .WithMessage("Password must contain at least one number")
        //     .Matches("[^a-zA-Z0-9]")
        //     .WithMessage("Password must contain at least one special character");

        RuleFor(x => x.Role)
            .Must(r => r == "user" || r == "admin")
            .WithMessage("Role must be 'user' or 'admin'");
    }
}
