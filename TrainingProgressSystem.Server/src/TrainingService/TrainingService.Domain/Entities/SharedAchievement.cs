using Shared.Kernal.Models;

namespace TrainingService.Domain.Entities;

public class SharedAchievement : BaseEntity
{
    public Guid AchievementId { get; set; }
    
    public required string PublicUrlKey { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? Expiration { get; set; }

    public Achievement Achievement { get; set; } = null!;
}