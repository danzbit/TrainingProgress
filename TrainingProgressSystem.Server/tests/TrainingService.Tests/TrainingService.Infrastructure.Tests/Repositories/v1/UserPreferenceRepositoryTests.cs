using Microsoft.EntityFrameworkCore;
using TrainingService.Domain.Entities;
using TrainingService.Infrastructure.Repositories.v1;
using TrainingService.Infrastructure.Tests.Infrastructure;

namespace TrainingService.Infrastructure.Tests.Repositories.v1;

[TestFixture]
public class UserPreferenceRepositoryTests
{
    [Test]
    public async Task GetByUserIdAsync_WhenPreferenceExists_ReturnsPreference()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var userId = Guid.NewGuid();

        db.UserPreferences.Add(new UserPreference
        {
            UserId = userId,
            HistoryViewMode = "calendar"
        });
        await db.SaveChangesAsync();

        var repository = new UserPreferenceRepository(db);

        var result = await repository.GetByUserIdAsync(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.HistoryViewMode, Is.EqualTo("calendar"));
    }

    [Test]
    public async Task UpsertAsync_WhenPreferenceIsNew_InsertsPreference()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var userId = Guid.NewGuid();
        var repository = new UserPreferenceRepository(db);

        var result = await repository.UpsertAsync(new UserPreference
        {
            UserId = userId,
            HistoryViewMode = "list"
        });

        Assert.That(result.IsFailure, Is.False);
        Assert.That(await db.UserPreferences.AnyAsync(p => p.UserId == userId), Is.True);
    }

    [Test]
    public async Task UpsertAsync_WhenPreferenceExists_UpdatesHistoryViewMode()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var userId = Guid.NewGuid();

        db.UserPreferences.Add(new UserPreference
        {
            UserId = userId,
            HistoryViewMode = "list"
        });
        await db.SaveChangesAsync();

        var repository = new UserPreferenceRepository(db);

        var result = await repository.UpsertAsync(new UserPreference
        {
            UserId = userId,
            HistoryViewMode = "calendar"
        });

        Assert.That(result.IsFailure, Is.False);

        var updated = await db.UserPreferences.FirstAsync(p => p.UserId == userId);
        Assert.That(updated.HistoryViewMode, Is.EqualTo("calendar"));
    }
}