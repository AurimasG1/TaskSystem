using FluentValidation;
using TaskSystem.Application.Commands.Uzduotys.UpdateUzduotis;

namespace TaskSystem.Application.Validation.Uzduotys;

public class UpdateUzduotisCommandValidator : AbstractValidator<UpdateUzduotisCommand>
{
    public UpdateUzduotisCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.StatusId).InclusiveBetween(1, 3);
    }
}
