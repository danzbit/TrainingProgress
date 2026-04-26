using Bff.Application.Dtos.v1.Commands;
using Bff.Application.Dtos.v1.Requests;

namespace Bff.Application.Maps.v1;

public static class CreateWorkoutSagaCommandToCreateWorkoutCommandMapper
{
    public static CreateWorkoutCommand ToCreateWorkoutCommand(this CreateWorkoutSagaCommand command, Guid userId)
    {
        return new CreateWorkoutCommand(
            userId,
            command.Date,
            command.WorkoutTypeId,
            command.DurationMin,
            command.Notes,
            command.Exercises?.Select(ex =>
                    new CreateWorkoutExerciseCommand(ex.ExerciseId, ex.ExerciseTypeId, ex.Sets, ex.Reps, ex.DurationSec,
                        ex.WeightKg))
                .ToList());
    }
}
