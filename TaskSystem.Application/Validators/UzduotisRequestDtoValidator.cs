using FluentValidation;
using TaskSystem.Application.DTO;

namespace TaskSystem.Application.Validators;

public class UzduotisRequestDtoValidator : AbstractValidator<UzduotisRequestDto>
{
    public UzduotisRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required").MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.StatusId).GreaterThan(0).WithMessage("StatusId must be between 1 and 3");
    }
}
