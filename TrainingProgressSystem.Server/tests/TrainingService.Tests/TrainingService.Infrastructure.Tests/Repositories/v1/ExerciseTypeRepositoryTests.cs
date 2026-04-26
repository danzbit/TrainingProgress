using TrainingService.Domain.Entities;
using TrainingService.Infrastructure.Repositories.v1;
using TrainingService.Infrastructure.Tests.Infrastructure;

namespace TrainingService.Infrastructure.Tests.Repositories.v1;

[TestFixture]
public class ExerciseTypeRepositoryTests
{
    [Test]
    public async Task GetAllAsync_ReturnsAllExerciseTypes()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        db.ExerciseTypes.AddRange(
            new ExerciseType { Id = Guid.NewGuid(), Name = "Bench Press", Category = "Strength" },
            new ExerciseType { Id = Guid.NewGuid(), Name = "Running", Category = "Cardio" });
        await db.SaveChangesAsync();

        var repository = new ExerciseTypeRepository(db);

        var result = await repository.GetAllAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Count, Is.EqualTo(2));
        Assert.That(result.Value.Select(x => x.Name), Is.EquivalentTo(new[] { "Bench Press", "Running" }));
    }
}