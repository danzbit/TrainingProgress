namespace TrainingService.Domain.Entities;

public class GoalProgress
{
    public Guid GoalId { get; set; }

    public int CurrentValue { get; set; }

    public double Percentage { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime LastCalculatedAt { get; set; }

    public Goal Goal { get; set; } = null!;
}