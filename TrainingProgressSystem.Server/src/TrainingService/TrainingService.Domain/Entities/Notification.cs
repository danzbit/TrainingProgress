using Shared.Infrastructure.Identity;
using Shared.Kernal.Models;

namespace TrainingService.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }

    public required string Type { get; set; }
    
    public required string Message { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ApplicationUser User { get; set; } = null!;
}