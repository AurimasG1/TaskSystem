using FluentValidation;
using TaskSystem.Application.Commands.Uzduotys.CreateUzduotis;

namespace TaskSystem.Application.Validation.Uzduotys;

public class CreateUzduotisCommandValidator : AbstractValidator<CreateUzduotisCommand>
{
    public CreateUzduotisCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
