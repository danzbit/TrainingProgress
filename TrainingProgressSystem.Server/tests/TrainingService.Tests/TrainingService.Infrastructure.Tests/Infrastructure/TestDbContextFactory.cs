using Microsoft.EntityFrameworkCore;
using TrainingService.Infrastructure.Data;

namespace TrainingService.Infrastructure.Tests.Infrastructure;

internal static class TestDbContextFactory
{
    public static TrainingServiceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TrainingServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new TrainingServiceDbContext(options);
    }
}