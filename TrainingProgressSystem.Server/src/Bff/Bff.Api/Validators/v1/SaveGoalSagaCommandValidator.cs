using Bff.Application.Dtos.v1.Requests;
using FluentValidation;

namespace Bff.Api.Validators.v1;

public sealed class SaveGoalSagaCommandValidator : AbstractValidator<SaveGoalSagaCommand>
{
    public SaveGoalSagaCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Description)
            .MaximumLength(1000);

        RuleFor(command => command.MetricType)
            .InclusiveBetween(0, 7);

        RuleFor(command => command.PeriodType)
            .InclusiveBetween(0, 3);

        RuleFor(command => command.TargetValue)
            .GreaterThan(0);

        RuleFor(command => command.StartDate)
            .NotEqual(default(DateTime));

        RuleFor(command => command)
            .Must(command => !command.EndDate.HasValue || command.EndDate.Value >= command.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}
