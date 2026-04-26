using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalyticsService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[AnalyticsSnapshots]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AnalyticsSnapshots] (
                        [Id] uniqueidentifier NOT NULL,
                        [UserId] uniqueidentifier NOT NULL,
                        [AmountPerWeek] int NOT NULL,
                        [WeekDurationMin] int NOT NULL,
                        [AmountThisMonth] int NOT NULL,
                        [MonthlyTimeMin] int NOT NULL,
                        [TotalAchievedGoals] int NOT NULL,
                        [TotalWorkoutsCompleted] int NOT NULL,
                        [WorkoutsThisWeek] int NOT NULL,
                        [TotalTrainingHours] float NOT NULL,
                        [DailyTrendJson] nvarchar(max) NOT NULL,
                        [CountByTypeJson] nvarchar(max) NOT NULL,
                        [LastCalculatedAtUtc] datetime2 NOT NULL,
                        CONSTRAINT [PK_AnalyticsSnapshots] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_AnalyticsSnapshots_UserId'
                      AND object_id = OBJECT_ID(N'[AnalyticsSnapshots]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_AnalyticsSnapshots_UserId]
                    ON [AnalyticsSnapshots] ([UserId]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[AnalyticsSnapshots]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [AnalyticsSnapshots];
                END
                """);
        }
    }
}
