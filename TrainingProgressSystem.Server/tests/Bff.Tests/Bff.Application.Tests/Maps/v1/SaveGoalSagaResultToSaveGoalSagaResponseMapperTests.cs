using Bff.Application.Dtos.v1.Responses;
using Bff.Application.Maps.v1;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace Bff.Application.Tests.Maps.v1;

[TestFixture]
public class SaveGoalSagaResultToSaveGoalSagaResponseMapperTests
{
    [Test]
    public void ToSaveGoalSagaResponse_WithSuccessResult_MapsGoalId()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var result = ResultOfT<SaveGoalSagaResult>.Success(
            new SaveGoalSagaResult(goalId));

        // Act
        var response = result.ToSaveGoalSagaResponse();

        // Assert
        Assert.That(response.GoalId, Is.EqualTo(goalId));
    }

    [Test]
    public void ToSaveGoalSagaResponse_WithFailureResult_MapsNullGoalId()
    {
        // Arrange
        var error = new Error(ErrorCode.UnexpectedError, "Goal creation failed");
        var resultValue = new SaveGoalSagaResult(null, "Goal creation failed");
        var result = ResultOfT<SaveGoalSagaResult>.Failure(resultValue, error);

        // Act
        var response = result.ToSaveGoalSagaResponse();

        // Assert
        Assert.That(response.GoalId, Is.Null);
    }

    [Test]
    public void ToSaveGoalSagaResponse_WithNullGoalId_MapsNullGoalId()
    {
        // Arrange
        var result = ResultOfT<SaveGoalSagaResult>.Success(
            new SaveGoalSagaResult(null));

        // Act
        var response = result.ToSaveGoalSagaResponse();

        // Assert
        Assert.That(response.GoalId, Is.Null);
    }

    [Test]
    public void ToSaveGoalSagaResponse_WithErrorMessage_MapsError()
    {
        // Arrange
        var errorMessage = "Partial failure - reminder scheduling failed";
        var result = ResultOfT<SaveGoalSagaResult>.Success(
            new SaveGoalSagaResult(null, errorMessage));

        // Act
        var response = result.ToSaveGoalSagaResponse();

        // Assert
        Assert.That(response.Error, Is.Null);
    }

    [Test]
    public void ToSaveGoalSagaResponse_WithValidGoalIdAndError_MapsGoalIdAndError()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var errorMessage = "Partial failure - notification failed";
        var result = ResultOfT<SaveGoalSagaResult>.Success(
            new SaveGoalSagaResult(goalId, errorMessage));

        // Act
        var response = result.ToSaveGoalSagaResponse();

        // Assert
        Assert.That(response.GoalId, Is.EqualTo(goalId));
        Assert.That(response.Error, Is.Null);
    }
}
