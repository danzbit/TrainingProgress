using AutoMapper;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Maps.v1;
using TrainingService.Domain.Entities;

namespace TrainingService.Application.Tests.Maps.v1;

[TestFixture]
public class WorkoutMappingProfileTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<WorkoutMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Test]
    public void Map_UpdateWorkoutRequestToWorkout_MapsWorkoutIdToId()
    {
        var workoutId = Guid.NewGuid();
        var request = new UpdateWorkoutRequest(
            workoutId,
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            20,
            "notes",
            []);

        var result = _mapper.Map<Workout>(request);

        Assert.That(result.Id, Is.EqualTo(workoutId));
    }

    [Test]
    public void Map_WorkoutTypeToWorkoutTypeResponse_MapsFields()
    {
        var entity = new WorkoutType
        {
            Id = Guid.NewGuid(),
            Name = "Cardio",
            Description = "desc"
        };

        var result = _mapper.Map<WorkoutTypeResponse>(entity);

        Assert.That(result.Id, Is.EqualTo(entity.Id));
        Assert.That(result.Name, Is.EqualTo(entity.Name));
        Assert.That(result.Description, Is.EqualTo(entity.Description));
    }
}
