using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Api.Middlewares;

namespace Shared.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationsAsync<TContext>(this WebApplication app, ILogger logger)
        where TContext : DbContext
    {
        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

                var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

                if (pendingMigrations.Count > 0)
                {
                    await BaselineInitialMigrationIfSchemaExistsAsync(dbContext, logger);
                    pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
                }

                var pendingCount = pendingMigrations.Count;

                if (pendingCount == 0)
                {
                    logger.LogInformation("No pending migrations for {DbContext}", typeof(TContext).Name);
                    return;
                }

                logger.LogInformation(
                    "Applying {PendingCount} pending migration(s) for {DbContext} (attempt {Attempt}/{MaxAttempts})",
                    pendingCount,
                    typeof(TContext).Name,
                    attempt,
                    maxAttempts);

                await dbContext.Database.MigrateAsync();

                logger.LogInformation("Successfully applied migrations for {DbContext}", typeof(TContext).Name);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Failed applying migrations for {DbContext} on attempt {Attempt}/{MaxAttempts}. Retrying...",
                    typeof(TContext).Name,
                    attempt,
                    maxAttempts);

                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        using var finalScope = app.Services.CreateScope();
        var finalContext = finalScope.ServiceProvider.GetRequiredService<TContext>();
        await finalContext.Database.MigrateAsync();
    }

    private static async Task BaselineInitialMigrationIfSchemaExistsAsync<TContext>(TContext dbContext, ILogger logger)
        where TContext : DbContext
    {
        var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToList();
        if (appliedMigrations.Count != 0)
        {
            return;
        }

        var allMigrations = dbContext.Database.GetMigrations().ToList();
        if (allMigrations.Count == 0)
        {
            return;
        }

        var initialMigrationId = allMigrations[0];

        var tableNames = dbContext.Model
            .GetEntityTypes()
            .Select(entityType => entityType.GetTableName())
            .Where(tableName => !string.IsNullOrWhiteSpace(tableName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (tableNames.Count == 0)
        {
            return;
        }

          var probeTableName = tableNames[0]!;

            var existingModelTableCount = await dbContext.Database.SqlQuery<int>($@"
    SELECT COUNT(*) AS [Value]
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
        AND TABLE_NAME = {probeTableName}").SingleAsync();

        if (existingModelTableCount == 0)
        {
            return;
        }

        var efVersion = typeof(DbContext).Assembly.GetName().Version;
        var productVersion = efVersion is null
            ? "9.0.0"
            : $"{efVersion.Major}.{efVersion.Minor}.{Math.Max(0, efVersion.Build)}";

        await dbContext.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END");

        await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {initialMigrationId})
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ({initialMigrationId}, {productVersion});
END");

        logger.LogWarning(
            "Detected existing schema for {DbContext} with empty EF migration history. Baseline-marked initial migration {MigrationId}.",
            typeof(TContext).Name,
            initialMigrationId);
    }

    public static void ConfigureWebApplication(this WebApplication app)
    {
        app.UseStatusCodePages();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
    public static void ConfigureSwagger(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI();
    }

    public static void ConfigureApiMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<IdempotencyMiddleware>();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
    }
}