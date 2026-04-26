using Shared.Infrastructure.Identity;
using Shared.Kernal.Models;

namespace AnalyticsService.Domain.Entities;

public class Workout : BaseEntity
{
    public Guid WorkoutTypeId { get; set; }

    public WorkoutType WorkoutType { get; set; } = null!;

    public DateTime Date { get; set; }

    public int DurationMin { get; set; }
    
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;
}