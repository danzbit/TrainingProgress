using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Data;
using Shared.Contracts.Idempotency;
using Shared.Infrastructure.Data.Configurations;
using Shared.Infrastructure.Identity;
using TrainingService.Domain.Entities;

namespace TrainingService.Infrastructure.Data;

public class TrainingServiceDbContext(DbContextOptions<TrainingServiceDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IDbContext
{
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
    
    public DbSet<Achievement> Achievements { get; set; }
    
    public DbSet<Goal> Goals { get; set; }

    public DbSet<GoalProgress> GoalProgresses { get; set; }
    
    public DbSet<Exercise> Exercises { get; set; }
    
    public DbSet<ExerciseType> ExerciseTypes { get; set; }
    
    public DbSet<Notification> Notifications { get; set; }
    
    public DbSet<SharedAchievement> SharedAchievements { get; set; }
    
    public DbSet<Workout> Workouts { get; set; }
    
    public DbSet<WorkoutType> WorkoutTypes { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdempotencyRecordConfiguration).Assembly);

        SeedWorkoutTypes(modelBuilder);
        SeedExerciseTypes(modelBuilder);
    }

    private static void SeedWorkoutTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutType>().HasData(
            new WorkoutType { Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"), Name = "Strength", Description = "Weight and resistance training" },
            new WorkoutType { Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"), Name = "Cardio", Description = "Cardiovascular endurance training" },
            new WorkoutType { Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"), Name = "Flexibility", Description = "Stretching and mobility work" },
            new WorkoutType { Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"), Name = "HIIT", Description = "High-intensity interval training" },
            new WorkoutType { Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"), Name = "Yoga", Description = "Yoga and mindful movement" },
            new WorkoutType { Id = Guid.Parse("a6666666-6666-6666-6666-666666666666"), Name = "Sports", Description = "Sport-specific activities" },
            new WorkoutType { Id = Guid.Parse("a7777777-7777-7777-7777-777777777777"), Name = "Other", Description = "Other physical activity" }
        );
    }

    private static void SeedExerciseTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciseType>().HasData(
            // Strength
            new ExerciseType { Id = Guid.Parse("b1111111-1111-1111-1111-111111111111"), Name = "Bench Press", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"), Name = "Squat", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("b3333333-3333-3333-3333-333333333333"), Name = "Deadlift", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Name = "Overhead Press", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("b5555555-5555-5555-5555-555555555555"), Name = "Pull-up", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("b6666666-6666-6666-6666-666666666666"), Name = "Barbell Row", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("b7777777-7777-7777-7777-777777777777"), Name = "Dumbbell Curl", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("b8888888-8888-8888-8888-888888888888"), Name = "Tricep Dip", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("b9999999-9999-9999-9999-999999999999"), Name = "Leg Press", Category = "Strength" },
            new ExerciseType { Id = Guid.Parse("baaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Lunges", Category = "Strength" },
            // Cardio
            new ExerciseType { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Running", Category = "Cardio" },
            new ExerciseType { Id = Guid.Parse("bccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Cycling", Category = "Cardio" },
            new ExerciseType { Id = Guid.Parse("bddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Rowing", Category = "Cardio" },
            new ExerciseType { Id = Guid.Parse("beeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Jump Rope", Category = "Cardio" },
            new ExerciseType { Id = Guid.Parse("bfffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Elliptical", Category = "Cardio" },
            // Flexibility
            new ExerciseType { Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"), Name = "Hip Flexor Stretch", Category = "Flexibility" },
            new ExerciseType { Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"), Name = "Hamstring Stretch", Category = "Flexibility" },
            new ExerciseType { Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"), Name = "Shoulder Stretch", Category = "Flexibility" },
            // Core
            new ExerciseType { Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"), Name = "Plank", Category = "Core" },
            new ExerciseType { Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"), Name = "Crunches", Category = "Core" },
            new ExerciseType { Id = Guid.Parse("c6666666-6666-6666-6666-666666666666"), Name = "Leg Raises", Category = "Core" },
            new ExerciseType { Id = Guid.Parse("c7777777-7777-7777-7777-777777777777"), Name = "Russian Twists", Category = "Core" }
        );
    }
}