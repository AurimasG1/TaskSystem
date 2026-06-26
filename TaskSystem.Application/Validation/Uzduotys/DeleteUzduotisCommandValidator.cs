using FluentValidation;
using TaskSystem.Application.Commands.Uzduotys.DeleteUzduotis;

public class DeleteUzduotisCommandValidator : AbstractValidator<DeleteUzduotisCommand>
{
    public DeleteUzduotisCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
