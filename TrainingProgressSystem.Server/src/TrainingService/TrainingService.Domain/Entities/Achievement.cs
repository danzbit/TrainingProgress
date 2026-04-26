using Shared.Infrastructure.Identity;
using Shared.Kernal.Models;

namespace TrainingService.Domain.Entities;

public class Achievement : BaseEntity
{
    public required string Title { get; set; }
    
    public string? Description { get; set; }
    
    public string? IconUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } = null!;

    public ICollection<SharedAchievement> SharedAchievements { get; set; } = new List<SharedAchievement>();
}