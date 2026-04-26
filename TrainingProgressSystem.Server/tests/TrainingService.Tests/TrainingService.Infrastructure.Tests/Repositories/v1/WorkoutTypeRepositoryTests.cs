using TrainingService.Domain.Entities;
using TrainingService.Infrastructure.Repositories.v1;
using TrainingService.Infrastructure.Tests.Infrastructure;

namespace TrainingService.Infrastructure.Tests.Repositories.v1;

[TestFixture]
public class WorkoutTypeRepositoryTests
{
    [Test]
    public async Task GetAllAsync_ReturnsAllWorkoutTypes()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        db.WorkoutTypes.AddRange(
            new WorkoutType { Id = Guid.NewGuid(), Name = "Cardio", Description = "desc" },
            new WorkoutType { Id = Guid.NewGuid(), Name = "Strength", Description = "desc" });
        await db.SaveChangesAsync();

        var repository = new WorkoutTypeRepository(db);

        var result = await repository.GetAllAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Count, Is.EqualTo(2));
        Assert.That(result.Value.Select(x => x.Name), Is.EquivalentTo(new[] { "Cardio", "Strength" }));
    }
}