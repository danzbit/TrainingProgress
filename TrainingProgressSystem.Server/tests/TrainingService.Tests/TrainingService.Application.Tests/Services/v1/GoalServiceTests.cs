using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Services.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Enums;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Tests.Services.v1;

[TestFixture]
public class GoalServiceTests
{
    [Test]
    public async Task CreateGoalAsync_WhenNameMissing_ReturnsValidationFailure()
    {
        var repositoryMock = new Mock<IGoalRepository>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<GoalService>>();

        var service = new GoalService(repositoryMock.Object, mapperMock.Object, loggerMock.Object);

        var request = new CreateGoalRequest(
            Guid.NewGuid(),
            " ",
            "desc",
            GoalMetricType.WorkoutCount,
            GoalPeriodType.Weekly,
            10,
            DateTime.UtcNow,
            null);

        var result = await service.CreateGoalAsync(request);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.ValidationFailed));
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Goal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetGoalAsync_WhenRepositoryReturnsNull_ReturnsEntityNotFound()
    {
        var repositoryMock = new Mock<IGoalRepository>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<GoalService>>();

        repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<Goal?>.Success(null));

        var service = new GoalService(repositoryMock.Object, mapperMock.Object, loggerMock.Object);

        var result = await service.GetGoalAsync(Guid.NewGuid());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(Error.EntityNotFound));
    }

    [Test]
    public async Task GetAllGoalsAsync_WhenCompletedGoalWithoutProgress_UsesFallbackValues()
    {
        var repositoryMock = new Mock<IGoalRepository>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<GoalService>>();

        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Goal",
            Description = "Description",
            MetricType = GoalMetricType.WorkoutCount,
            PeriodType = GoalPeriodType.Weekly,
            TargetValue = 7,
            Status = GoalStatus.Completed,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(7),
            Progress = null
        };

        repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<Goal>>.Success([goal]));

        var service = new GoalService(repositoryMock.Object, mapperMock.Object, loggerMock.Object);

        var result = await service.GetAllGoalsAsync();

        Assert.That(result.IsFailure, Is.False);
        var item = result.Value.Single();
        Assert.That(item.ProgressInfo.CurrentValue, Is.EqualTo(goal.TargetValue));
        Assert.That(item.ProgressInfo.ProgressPercentage, Is.EqualTo(100d));
        Assert.That(item.ProgressInfo.IsCompleted, Is.True);
    }

    [Test]
    public async Task RecalculateProgressForGoalAsync_WhenRepositoryFails_ReturnsFailure()
    {
        var repositoryMock = new Mock<IGoalRepository>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<GoalService>>();
        var error = new Error(ErrorCode.UnexpectedError, "failed");

        repositoryMock
            .Setup(x => x.RecalculateGoalsProgressAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<int>.Failure(error));

        var service = new GoalService(repositoryMock.Object, mapperMock.Object, loggerMock.Object);

        var result = await service.RecalculateProgressForGoalAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(error));
    }
}
