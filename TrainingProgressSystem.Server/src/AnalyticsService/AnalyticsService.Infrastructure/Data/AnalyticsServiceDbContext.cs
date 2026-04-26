using AnalyticsService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions.Data;
using Shared.Contracts.Idempotency;
using Shared.Infrastructure.Data.Configurations;
using Shared.Infrastructure.Identity;

namespace AnalyticsService.Infrastructure.Data;

public class AnalyticsServiceDbContext(DbContextOptions<AnalyticsServiceDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IDbContext
{
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

    public DbSet<AnalyticsSnapshot> AnalyticsSnapshots { get; set; }
    
    public DbSet<Goal> Goals { get; set; }

    public DbSet<Workout> Workouts { get; set; }

    public DbSet<WorkoutType> WorkoutTypes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdempotencyRecordConfiguration).Assembly);
    }
}