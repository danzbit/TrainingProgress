using Shared.Grpc.Contracts;
using TrainingService.Api.Maps.v1;

namespace TrainingService.Api.Tests.Maps.v1;

[TestFixture]
public class CreateWorkoutExerciseGrpcRequestToExerciseRequestMapperTests
{
    [Test]
    public void ToExerciseRequests_WhenInputHasValidAndInvalidItems_MapsOnlyValidItems()
    {
        var validExerciseTypeId = Guid.NewGuid();
        var validExerciseId = Guid.NewGuid();

        var input = new List<CreateWorkoutExerciseGrpcRequest>
        {
            new()
            {
                ExerciseTypeId = validExerciseTypeId.ToString(),
                ExerciseId = validExerciseId.ToString(),
                HasExerciseId = true,
                Sets = 3,
                Reps = 10,
                DurationSec = 120,
                HasDurationSec = true,
                WeightKg = 55.5f,
                HasWeightKg = true
            },
            new()
            {
                ExerciseTypeId = "invalid-guid",
                Sets = 4,
                Reps = 8
            }
        };

        var result = input.ToExerciseRequests();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result![0].ExerciseTypeId, Is.EqualTo(validExerciseTypeId));
        Assert.That(result[0].ExerciseId, Is.EqualTo(validExerciseId));
        Assert.That(result[0].DurationSec, Is.EqualTo(120));
        Assert.That(result[0].WeightKg, Is.EqualTo(55.5m));
    }

    [Test]
    public void ToExerciseRequests_WhenNoValidItems_ReturnsNull()
    {
        var input = new List<CreateWorkoutExerciseGrpcRequest>
        {
            new() { ExerciseTypeId = "invalid" },
            new() { ExerciseTypeId = string.Empty }
        };

        var result = input.ToExerciseRequests();

        Assert.That(result, Is.Null);
    }
}
