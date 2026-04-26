using AutoMapper;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Maps.v1;
using TrainingService.Domain.Entities;

namespace TrainingService.Application.Tests.Maps.v1;

[TestFixture]
public class ExerciseMappingProfileTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ExerciseMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Test]
    public void Map_ExerciseRequestToExercise_MapsExerciseIdToId()
    {
        var exerciseId = Guid.NewGuid();
        var exerciseTypeId = Guid.NewGuid();

        var request = new ExerciseRequest(exerciseId, exerciseTypeId, 3, 12, 60, 25m);

        var result = _mapper.Map<Exercise>(request);

        Assert.That(result.Id, Is.EqualTo(exerciseId));
        Assert.That(result.ExerciseTypeId, Is.EqualTo(exerciseTypeId));
        Assert.That(result.Sets, Is.EqualTo(3));
    }

    [Test]
    public void Map_ExerciseTypeToExerciseTypeResponse_MapsFields()
    {
        var entity = new ExerciseType
        {
            Id = Guid.NewGuid(),
            Name = "Bench Press",
            Category = "Strength"
        };

        var result = _mapper.Map<ExerciseTypeResponse>(entity);

        Assert.That(result.Id, Is.EqualTo(entity.Id));
        Assert.That(result.Name, Is.EqualTo("Bench Press"));
        Assert.That(result.Category, Is.EqualTo("Strength"));
    }
}
