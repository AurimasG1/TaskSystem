using FluentValidation;
using TaskSystem.Application.Commands.Uzduotys.UzduotisCreate;

namespace TaskSystem.Application.Validation.Uzduotys;

public class CreateUzduotisCommandValidator : AbstractValidator<UzduotisCreateCommand>
{
    public CreateUzduotisCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.UserProfileId).GreaterThan(0);
    }
}
