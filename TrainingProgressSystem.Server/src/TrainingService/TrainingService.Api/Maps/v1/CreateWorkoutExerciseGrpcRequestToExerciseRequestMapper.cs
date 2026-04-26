using Shared.Grpc.Contracts;
using TrainingService.Application.Dtos.v1.Requests;

namespace TrainingService.Api.Maps.v1;

public static class CreateWorkoutExerciseGrpcRequestToExerciseRequestMapper
{
    public static List<ExerciseRequest>? ToExerciseRequests(this IEnumerable<CreateWorkoutExerciseGrpcRequest> exercises)
    {
        var list = new List<ExerciseRequest>();

        foreach (var exercise in exercises)
        {
            if (!Guid.TryParse(exercise.ExerciseTypeId, out var exerciseTypeId))
            {
                continue;
            }

            Guid? exerciseId = null;
            if (exercise.HasExerciseId && Guid.TryParse(exercise.ExerciseId, out var parsedExerciseId))
            {
                exerciseId = parsedExerciseId;
            }

            list.Add(new ExerciseRequest(
                exerciseId,
                exerciseTypeId,
                exercise.Sets,
                exercise.Reps,
                exercise.HasDurationSec ? exercise.DurationSec : null,
                exercise.HasWeightKg ? Convert.ToDecimal(exercise.WeightKg) : null));
        }

        return list.Count == 0 ? null : list;
    }
}
