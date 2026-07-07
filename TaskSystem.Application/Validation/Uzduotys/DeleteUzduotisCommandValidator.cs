using FluentValidation;
using TaskSystem.Application.Commands.Uzduotys.UzduotisDelete;

public class DeleteUzduotisCommandValidator : AbstractValidator<UzduotisDeleteCommand>
{
    public DeleteUzduotisCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.UserProfileId).GreaterThan(0);
    }
}
