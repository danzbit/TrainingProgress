using Microsoft.EntityFrameworkCore;
using TrainingService.Domain.Entities;
using TrainingService.Infrastructure.Repositories.v1;
using TrainingService.Infrastructure.Tests.Infrastructure;

namespace TrainingService.Infrastructure.Tests.Repositories.v1;

[TestFixture]
public class AchievementRepositoryTests
{
    [Test]
    public async Task AddAchievementAsync_PersistsAchievement()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new AchievementRepository(db);
        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "First Workout",
            Description = "Completed first session",
            CreatedAt = DateTime.UtcNow
        };

        var result = await repository.AddAchievementAsync(achievement);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(await db.Achievements.AnyAsync(a => a.Id == achievement.Id), Is.True);
    }

    [Test]
    public async Task AddSharedAchievementAsync_PersistsSharedAchievement()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var repository = new AchievementRepository(db);
        var achievement = await SeedAchievementAsync(db, Guid.NewGuid());
        var shared = new SharedAchievement
        {
            Id = Guid.NewGuid(),
            AchievementId = achievement.Id,
            PublicUrlKey = "public-1",
            CreatedAt = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.AddDays(1)
        };

        var result = await repository.AddSharedAchievementAsync(shared);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(await db.SharedAchievements.AnyAsync(sa => sa.Id == shared.Id), Is.True);
    }

    [Test]
    public async Task GetActiveSharedAchievementByUserIdAsync_ReturnsNewestNonExpiredRecord()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var userId = Guid.NewGuid();
        var achievement = await SeedAchievementAsync(db, userId);

        db.SharedAchievements.AddRange(
            new SharedAchievement
            {
                Id = Guid.NewGuid(),
                AchievementId = achievement.Id,
                PublicUrlKey = "expired",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Expiration = DateTime.UtcNow.AddDays(-1)
            },
            new SharedAchievement
            {
                Id = Guid.NewGuid(),
                AchievementId = achievement.Id,
                PublicUrlKey = "older-active",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                Expiration = DateTime.UtcNow.AddHours(3)
            },
            new SharedAchievement
            {
                Id = Guid.NewGuid(),
                AchievementId = achievement.Id,
                PublicUrlKey = "latest-active",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Expiration = DateTime.UtcNow.AddHours(4)
            });

        await db.SaveChangesAsync();

        var repository = new AchievementRepository(db);

        var result = await repository.GetActiveSharedAchievementByUserIdAsync(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.PublicUrlKey, Is.EqualTo("latest-active"));
    }

    [Test]
    public async Task GetActiveSharedAchievementByKeyAsync_WhenExpired_ReturnsNull()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var achievement = await SeedAchievementAsync(db, Guid.NewGuid());

        db.SharedAchievements.Add(new SharedAchievement
        {
            Id = Guid.NewGuid(),
            AchievementId = achievement.Id,
            PublicUrlKey = "expired-key",
            CreatedAt = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.AddMinutes(-1)
        });
        await db.SaveChangesAsync();

        var repository = new AchievementRepository(db);

        var result = await repository.GetActiveSharedAchievementByKeyAsync("expired-key");

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public async Task GetActiveSharedAchievementByKeyAsync_WhenActive_ReturnsSharedAchievement()
    {
        await using var db = TestDbContextFactory.CreateDbContext();
        var achievement = await SeedAchievementAsync(db, Guid.NewGuid());

        db.SharedAchievements.Add(new SharedAchievement
        {
            Id = Guid.NewGuid(),
            AchievementId = achievement.Id,
            PublicUrlKey = "active-key",
            CreatedAt = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.AddHours(1)
        });
        await db.SaveChangesAsync();

        var repository = new AchievementRepository(db);

        var result = await repository.GetActiveSharedAchievementByKeyAsync("active-key");

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Achievement, Is.Not.Null);
        Assert.That(result.Value.PublicUrlKey, Is.EqualTo("active-key"));
    }

    private static async Task<Achievement> SeedAchievementAsync(
        TrainingService.Infrastructure.Data.TrainingServiceDbContext db,
        Guid userId)
    {
        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Milestone",
            Description = "desc",
            CreatedAt = DateTime.UtcNow
        };

        db.Achievements.Add(achievement);
        await db.SaveChangesAsync();

        return achievement;
    }
}