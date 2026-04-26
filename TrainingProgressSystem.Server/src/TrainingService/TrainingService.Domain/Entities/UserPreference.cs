using Shared.Infrastructure.Identity;

namespace TrainingService.Domain.Entities;

public class UserPreference
{
    public Guid UserId { get; set; }
    public string HistoryViewMode { get; set; } = "list";
    public ApplicationUser User { get; set; } = null!;
}
