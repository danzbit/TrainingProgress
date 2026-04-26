using Bff.Application.Dtos.v1.Responses;
using Bff.Application.Maps.v1;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace Bff.Application.Tests.Maps.v1;

[TestFixture]
public class CreateWorkoutSagaResultToCreateWorkoutSagaResponseMapperTests
{
    [Test]
    public void ToCreateWorkoutSagaResponse_WithSuccessResult_MapsWorkoutId()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var result = ResultOfT<CreateWorkoutSagaResult>.Success(
            new CreateWorkoutSagaResult(workoutId));

        // Act
        var response = result.ToCreateWorkoutSagaResponse();

        // Assert
        Assert.That(response.WorkoutId, Is.EqualTo(workoutId));
    }

    [Test]
    public void ToCreateWorkoutSagaResponse_WithFailureResult_MapsNullWorkoutId()
    {
        // Arrange
        var error = new Error(ErrorCode.UnexpectedError, "Workout creation failed");
        var resultValue = new CreateWorkoutSagaResult(null, "Workout creation failed");
        var result = ResultOfT<CreateWorkoutSagaResult>.Failure(resultValue, error);

        // Act
        var response = result.ToCreateWorkoutSagaResponse();

        // Assert
        Assert.That(response.WorkoutId, Is.Null);
    }

    [Test]
    public void ToCreateWorkoutSagaResponse_WithNullWorkoutId_MapsNullWorkoutId()
    {
        // Arrange
        var result = ResultOfT<CreateWorkoutSagaResult>.Success(
            new CreateWorkoutSagaResult(null));

        // Act
        var response = result.ToCreateWorkoutSagaResponse();

        // Assert
        Assert.That(response.WorkoutId, Is.Null);
    }

    [Test]
    public void ToCreateWorkoutSagaResponse_WithErrorMessage_MapsError()
    {
        // Arrange
        var errorMessage = "Partial failure - notification failed";
        var result = ResultOfT<CreateWorkoutSagaResult>.Success(
            new CreateWorkoutSagaResult(null, errorMessage));

        // Act
        var response = result.ToCreateWorkoutSagaResponse();

        // Assert
        Assert.That(response.Error, Is.Null);
    }

    [Test]
    public void ToCreateWorkoutSagaResponse_WithValidWorkoutIdAndError_MapsWorkoutIdAndError()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var errorMessage = "Partial failure - analytics failed";
        var result = ResultOfT<CreateWorkoutSagaResult>.Success(
            new CreateWorkoutSagaResult(workoutId, errorMessage));

        // Act
        var response = result.ToCreateWorkoutSagaResponse();

        // Assert
        Assert.That(response.WorkoutId, Is.EqualTo(workoutId));
        Assert.That(response.Error, Is.Null);
    }
}
