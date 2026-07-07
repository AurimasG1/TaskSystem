using FluentValidation;
using TaskSystem.Application.Commands.Uzduotys.UzduotisUpdate;

namespace TaskSystem.Application.Validation.Uzduotys;

public class UpdateUzduotisCommandValidator : AbstractValidator<UzduotisUpdateCommand>
{
    public UpdateUzduotisCommandValidator()
    {
        // Title
        When(
            x => x.Title.HasValue,
            () =>
            {
                RuleFor(x => x.Title.Value).NotEmpty().MaximumLength(100);
            }
        );

        // Description
        When(
            x => x.Description.HasValue,
            () =>
            {
                RuleFor(x => x.Description.Value).MaximumLength(500);
            }
        );

        // StatusId
        When(
            x => x.StatusId.HasValue,
            () =>
            {
                RuleFor(x => x.StatusId.Value).InclusiveBetween(1, 3);
            }
        );
    }
}
