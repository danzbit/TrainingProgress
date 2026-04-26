using Shared.Kernal.Models;

namespace AnalyticsService.Domain.Entities;

public class Goal : BaseEntity
{
    public Guid UserId { get; set; }

    public int Status { get; set; }
}
