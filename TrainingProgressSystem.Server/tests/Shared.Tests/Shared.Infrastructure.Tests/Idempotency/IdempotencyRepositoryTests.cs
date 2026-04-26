using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Data;
using Shared.Contracts.Idempotency;
using Shared.Infrastructure.Data.Configurations;
using Shared.Infrastructure.Idempotency;

namespace Shared.Infrastructure.Tests.Idempotency;

[TestFixture]
public class IdempotencyRepositoryTests
{
    [Test]
    public async Task GetByKeyAsync_WhenRecordExists_ReturnsMatchingRecord()
    {
        await using var dbContext = CreateDbContext();
        var repository = new IdempotencyRepository(dbContext);

        var record = CreateRecord("POST", "/api/v1/workouts", "idem-key-1");
        dbContext.IdempotencyRecords.Add(record);
        await dbContext.SaveChangesAsync();

        var result = await repository.GetByKeyAsync("POST", "/api/v1/workouts", "idem-key-1");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IdempotencyKey, Is.EqualTo("idem-key-1"));
        Assert.That(result.Method, Is.EqualTo("POST"));
        Assert.That(result.Path, Is.EqualTo("/api/v1/workouts"));
    }

    [Test]
    public async Task GetByKeyAsync_WhenRecordDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var repository = new IdempotencyRepository(dbContext);

        dbContext.IdempotencyRecords.Add(CreateRecord("POST", "/api/v1/workouts", "idem-key-1"));
        await dbContext.SaveChangesAsync();

        var result = await repository.GetByKeyAsync("GET", "/api/v1/workouts", "idem-key-1");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SaveAsync_PersistsRecord()
    {
        await using var dbContext = CreateDbContext();
        var repository = new IdempotencyRepository(dbContext);

        var record = CreateRecord("PUT", "/api/v1/goals", "idem-key-2");

        await repository.SaveAsync(record);

        var stored = await dbContext.IdempotencyRecords.SingleAsync(r =>
            r.Method == "PUT" &&
            r.Path == "/api/v1/goals" &&
            r.IdempotencyKey == "idem-key-2");

        Assert.That(stored.StatusCode, Is.EqualTo(200));
        Assert.That(stored.ResponseBody, Is.EqualTo("{\"ok\":true}"));
        Assert.That(stored.Headers.ContainsKey("content-type"), Is.True);
        Assert.That(stored.Headers["content-type"], Is.EqualTo("application/json"));
    }

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private static IdempotencyRecord CreateRecord(string method, string path, string key)
    {
        return new IdempotencyRecord(
            key,
            method,
            path,
            200,
            "{\"ok\":true}",
            new Dictionary<string, string>
            {
                ["content-type"] = "application/json"
            },
            DateTime.UtcNow);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IDbContext
    {
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdempotencyRecordConfiguration).Assembly);
        }
    }
}