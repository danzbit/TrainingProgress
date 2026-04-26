using Shared.Kernal.Models;

namespace TrainingService.Domain.Entities;

public class WorkoutType : BaseEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<Workout> Workouts { get; set; } = [];
}