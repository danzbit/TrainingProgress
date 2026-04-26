using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrainingService.Infrastructure.Migrations._20260418135744_SeedWorkoutAndExerciseTypes
{
    /// <inheritdoc />
    public partial class SeedWorkoutAndExerciseTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO [ExerciseTypes] ([Id], [Category], [Name])
SELECT v.[Id], v.[Category], v.[Name]
FROM (VALUES
    (CAST('b1111111-1111-1111-1111-111111111111' AS uniqueidentifier), N'Strength', N'Bench Press'),
    (CAST('b2222222-2222-2222-2222-222222222222' AS uniqueidentifier), N'Strength', N'Squat'),
    (CAST('b3333333-3333-3333-3333-333333333333' AS uniqueidentifier), N'Strength', N'Deadlift'),
    (CAST('b4444444-4444-4444-4444-444444444444' AS uniqueidentifier), N'Strength', N'Overhead Press'),
    (CAST('b5555555-5555-5555-5555-555555555555' AS uniqueidentifier), N'Strength', N'Pull-up'),
    (CAST('b6666666-6666-6666-6666-666666666666' AS uniqueidentifier), N'Strength', N'Barbell Row'),
    (CAST('b7777777-7777-7777-7777-777777777777' AS uniqueidentifier), N'Strength', N'Dumbbell Curl'),
    (CAST('b8888888-8888-8888-8888-888888888888' AS uniqueidentifier), N'Strength', N'Tricep Dip'),
    (CAST('b9999999-9999-9999-9999-999999999999' AS uniqueidentifier), N'Strength', N'Leg Press'),
    (CAST('baaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' AS uniqueidentifier), N'Strength', N'Lunges'),
    (CAST('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb' AS uniqueidentifier), N'Cardio', N'Running'),
    (CAST('bccccccc-cccc-cccc-cccc-cccccccccccc' AS uniqueidentifier), N'Cardio', N'Cycling'),
    (CAST('bddddddd-dddd-dddd-dddd-dddddddddddd' AS uniqueidentifier), N'Cardio', N'Rowing'),
    (CAST('beeeeeee-eeee-eeee-eeee-eeeeeeeeeeee' AS uniqueidentifier), N'Cardio', N'Jump Rope'),
    (CAST('bfffffff-ffff-ffff-ffff-ffffffffffff' AS uniqueidentifier), N'Cardio', N'Elliptical'),
    (CAST('c1111111-1111-1111-1111-111111111111' AS uniqueidentifier), N'Flexibility', N'Hip Flexor Stretch'),
    (CAST('c2222222-2222-2222-2222-222222222222' AS uniqueidentifier), N'Flexibility', N'Hamstring Stretch'),
    (CAST('c3333333-3333-3333-3333-333333333333' AS uniqueidentifier), N'Flexibility', N'Shoulder Stretch'),
    (CAST('c4444444-4444-4444-4444-444444444444' AS uniqueidentifier), N'Core', N'Plank'),
    (CAST('c5555555-5555-5555-5555-555555555555' AS uniqueidentifier), N'Core', N'Crunches'),
    (CAST('c6666666-6666-6666-6666-666666666666' AS uniqueidentifier), N'Core', N'Leg Raises'),
    (CAST('c7777777-7777-7777-7777-777777777777' AS uniqueidentifier), N'Core', N'Russian Twists')
) AS v([Id], [Category], [Name])
WHERE NOT EXISTS (
    SELECT 1 FROM [ExerciseTypes] e WHERE e.[Id] = v.[Id]
);");

            migrationBuilder.Sql(@"
INSERT INTO [WorkoutTypes] ([Id], [Description], [Name])
SELECT v.[Id], v.[Description], v.[Name]
FROM (VALUES
    (CAST('a1111111-1111-1111-1111-111111111111' AS uniqueidentifier), N'Weight and resistance training', N'Strength'),
    (CAST('a2222222-2222-2222-2222-222222222222' AS uniqueidentifier), N'Cardiovascular endurance training', N'Cardio'),
    (CAST('a3333333-3333-3333-3333-333333333333' AS uniqueidentifier), N'Stretching and mobility work', N'Flexibility'),
    (CAST('a4444444-4444-4444-4444-444444444444' AS uniqueidentifier), N'High-intensity interval training', N'HIIT'),
    (CAST('a5555555-5555-5555-5555-555555555555' AS uniqueidentifier), N'Yoga and mindful movement', N'Yoga'),
    (CAST('a6666666-6666-6666-6666-666666666666' AS uniqueidentifier), N'Sport-specific activities', N'Sports'),
    (CAST('a7777777-7777-7777-7777-777777777777' AS uniqueidentifier), N'Other physical activity', N'Other')
) AS v([Id], [Description], [Name])
WHERE NOT EXISTS (
    SELECT 1 FROM [WorkoutTypes] w WHERE w.[Id] = v.[Id]
);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b6666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b7777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b8888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("b9999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("baaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("bccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("bddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("beeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("bfffffff-ffff-ffff-ffff-ffffffffffff"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("c4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("c5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("c6666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "ExerciseTypes",
                keyColumn: "Id",
                keyValue: new Guid("c7777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "WorkoutTypes",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "WorkoutTypes",
                keyColumn: "Id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "WorkoutTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "WorkoutTypes",
                keyColumn: "Id",
                keyValue: new Guid("a4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "WorkoutTypes",
                keyColumn: "Id",
                keyValue: new Guid("a5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "WorkoutTypes",
                keyColumn: "Id",
                keyValue: new Guid("a6666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "WorkoutTypes",
                keyColumn: "Id",
                keyValue: new Guid("a7777777-7777-7777-7777-777777777777"));
        }
    }
}
