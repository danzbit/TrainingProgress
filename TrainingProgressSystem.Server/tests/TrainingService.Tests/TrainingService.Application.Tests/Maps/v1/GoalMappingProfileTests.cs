using AutoMapper;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Maps.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Enums;

namespace TrainingService.Application.Tests.Maps.v1;

[TestFixture]
public class GoalMappingProfileTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<GoalMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Test]
    public void Map_UpdateGoalRequestToGoal_MapsGoalIdToId()
    {
        var goalId = Guid.NewGuid();
        var request = new UpdateGoalRequest(
            goalId,
            Guid.NewGuid(),
            "Lose Weight",
            "desc",
            GoalMetricType.WorkoutCount,
            GoalPeriodType.Monthly,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        var result = _mapper.Map<Goal>(request);

        Assert.That(result.Id, Is.EqualTo(goalId));
        Assert.That(result.Name, Is.EqualTo("Lose Weight"));
    }

    [Test]
    public void Map_GoalToGoalsResponse_WhenCompletedAndNoProgress_UsesFallbackValues()
    {
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Goal",
            Description = "desc",
            MetricType = GoalMetricType.WorkoutCount,
            PeriodType = GoalPeriodType.Weekly,
            TargetValue = 6,
            Status = GoalStatus.Completed,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(1),
            Progress = null
        };

        var result = _mapper.Map<GoalsResponse>(goal);

        Assert.That(result.CurrentValue, Is.EqualTo(6));
        Assert.That(result.ProgressPercentage, Is.EqualTo(100d));
        Assert.That(result.IsCompleted, Is.True);
        Assert.That(result.LastCalculatedAt, Is.Null);
    }
}
