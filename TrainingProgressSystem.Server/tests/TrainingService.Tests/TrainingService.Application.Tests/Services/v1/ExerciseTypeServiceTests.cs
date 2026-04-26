using Microsoft.Extensions.Logging;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Application.Services.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Tests.Services.v1;

[TestFixture]
public class ExerciseTypeServiceTests
{
    [Test]
    public async Task GetAllExerciseTypesAsync_WhenRepositoryFails_ReturnsFailure()
    {
        var repositoryMock = new Mock<IExerciseTypeRepository>();
        var loggerMock = new Mock<ILogger<ExerciseTypeService>>();
        var error = new Error(ErrorCode.UnexpectedError, "repo failed");

        repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<ExerciseType>>.Failure(error));

        var service = new ExerciseTypeService(repositoryMock.Object, loggerMock.Object);

        var result = await service.GetAllExerciseTypesAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(error));
    }

    [Test]
    public async Task GetAllExerciseTypesAsync_WhenRepositorySucceeds_MapsToResponse()
    {
        var repositoryMock = new Mock<IExerciseTypeRepository>();
        var loggerMock = new Mock<ILogger<ExerciseTypeService>>();

        var entities = new List<ExerciseType>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Squat",
                Category = "Strength"
            }
        };

        repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<ExerciseType>>.Success(entities));

        var service = new ExerciseTypeService(repositoryMock.Object, loggerMock.Object);

        var result = await service.GetAllExerciseTypesAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Value[0].Name, Is.EqualTo("Squat"));
        Assert.That(result.Value[0].Category, Is.EqualTo("Strength"));
    }
}
