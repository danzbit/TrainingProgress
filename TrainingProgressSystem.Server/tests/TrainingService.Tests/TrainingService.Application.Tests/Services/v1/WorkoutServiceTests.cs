using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Services.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Tests.Services.v1;

[TestFixture]
public class WorkoutServiceTests
{
    [Test]
    public async Task GetAllWorkoutsAsync_WhenRepositorySucceeds_MapsToListItems()
    {
        var repositoryMock = new Mock<IWorkoutRepository>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<WorkoutService>>();

        var workouts = new List<Workout>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow)
            {
                DurationMin = 30,
                WorkoutType = new WorkoutType { Name = "Cardio" },
                Exercises = [new Exercise(Guid.NewGuid(), 3, 10)]
            }
        };

        repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<Workout>>.Success(workouts));

        var service = new WorkoutService(repositoryMock.Object, mapperMock.Object, loggerMock.Object);

        var result = await service.GetAllWorkoutsAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Value[0].WorkoutType, Is.EqualTo("Cardio"));
        Assert.That(result.Value[0].AmountOfExercises, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateWorkoutAsync_WhenRepositorySucceeds_ReturnsCreatedWorkoutId()
    {
        var repositoryMock = new Mock<IWorkoutRepository>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<WorkoutService>>();
        var createdId = Guid.NewGuid();

        repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
            .Callback<Workout, CancellationToken>((workout, _) => workout.Id = createdId)
            .ReturnsAsync(Result.Success());

        var request = new CreateWorkoutRequest(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            45,
            "Run",
            [new ExerciseRequest(null, Guid.NewGuid(), 4, 12, 60, 20m)]);

        var service = new WorkoutService(repositoryMock.Object, mapperMock.Object, loggerMock.Object);

        var result = await service.CreateWorkoutAsync(request);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.WorkoutId, Is.EqualTo(createdId));
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetWorkoutAsync_WhenRepositoryFails_ReturnsFailure()
    {
        var repositoryMock = new Mock<IWorkoutRepository>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<WorkoutService>>();
        var error = new Error(ErrorCode.UnexpectedError, "failed");

        repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<Workout?>.Failure(error));

        var service = new WorkoutService(repositoryMock.Object, mapperMock.Object, loggerMock.Object);

        var result = await service.GetWorkoutAsync(Guid.NewGuid());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(error));
    }
}
