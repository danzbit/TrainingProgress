using Bff.Application.Dtos.v1.Requests;
using FluentValidation;

namespace Bff.Api.Validators.v1;

public sealed class CreateWorkoutSagaCommandValidator : AbstractValidator<CreateWorkoutSagaCommand>
{
    public CreateWorkoutSagaCommandValidator()
    {
        RuleFor(command => command.Date)
            .NotEqual(default(DateTime));

        RuleFor(command => command.WorkoutTypeId)
            .NotEmpty();

        RuleFor(command => command.DurationMin)
            .GreaterThan(0)
            .When(command => command.DurationMin.HasValue);

        RuleFor(command => command.Notes)
            .MaximumLength(1000)
            .When(command => !string.IsNullOrWhiteSpace(command.Notes));

        RuleForEach(command => command.Exercises)
            .SetValidator(new CreateWorkoutExerciseSagaCommandValidator())
            .When(command => command.Exercises is not null);
    }

    private sealed class CreateWorkoutExerciseSagaCommandValidator : AbstractValidator<CreateWorkoutExerciseSagaCommand>
    {
        public CreateWorkoutExerciseSagaCommandValidator()
        {
            RuleFor(exercise => exercise.ExerciseTypeId)
                .NotEmpty();

            RuleFor(exercise => exercise.Sets)
                .GreaterThan(0);

            RuleFor(exercise => exercise.Reps)
                .GreaterThan(0);

            RuleFor(exercise => exercise.DurationSec)
                .GreaterThan(0)
                .When(exercise => exercise.DurationSec.HasValue);

            RuleFor(exercise => exercise.WeightKg)
                .GreaterThanOrEqualTo(0)
                .When(exercise => exercise.WeightKg.HasValue);
        }
    }
}
