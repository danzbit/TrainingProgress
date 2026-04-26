using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using Shared.Abstractions.Data;
using Shared.Contracts.Idempotency;
using Shared.Infrastructure.Data.Configurations;
using Shared.Infrastructure.Identity;

namespace NotificationService.Infrastructure.Data;

public class NotificationServiceDbContext(DbContextOptions<NotificationServiceDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IDbContext
{
    public DbSet<Goal> Goals { get; set; }
    public DbSet<GoalProgress> GoalProgresses { get; set; }
    public DbSet<GoalReminder> GoalReminders { get; set; }

    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdempotencyRecordConfiguration).Assembly);
    }
}