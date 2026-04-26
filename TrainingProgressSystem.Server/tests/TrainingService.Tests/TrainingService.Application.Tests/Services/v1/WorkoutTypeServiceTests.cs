using Microsoft.Extensions.Logging;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Application.Services.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Tests.Services.v1;

[TestFixture]
public class WorkoutTypeServiceTests
{
    [Test]
    public async Task GetAllWorkoutTypesAsync_WhenRepositoryFails_ReturnsFailure()
    {
        var repositoryMock = new Mock<IWorkoutTypeRepository>();
        var loggerMock = new Mock<ILogger<WorkoutTypeService>>();
        var error = new Error(ErrorCode.UnexpectedError, "repo failed");

        repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutType>>.Failure(error));

        var service = new WorkoutTypeService(repositoryMock.Object, loggerMock.Object);

        var result = await service.GetAllWorkoutTypesAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(error));
    }

    [Test]
    public async Task GetAllWorkoutTypesAsync_WhenRepositorySucceeds_MapsToResponse()
    {
        var repositoryMock = new Mock<IWorkoutTypeRepository>();
        var loggerMock = new Mock<ILogger<WorkoutTypeService>>();

        var entities = new List<WorkoutType>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Strength",
                Description = "Heavy lifting"
            }
        };

        repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutType>>.Success(entities));

        var service = new WorkoutTypeService(repositoryMock.Object, loggerMock.Object);

        var result = await service.GetAllWorkoutTypesAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Value[0].Name, Is.EqualTo("Strength"));
        Assert.That(result.Value[0].Description, Is.EqualTo("Heavy lifting"));
    }
}
