using Shared.Kernal.Models;

namespace TrainingService.Domain.Entities;

public class ExerciseType : BaseEntity
{
    public required string Name { get; set; }
    
    public required string Category { get; set; }

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}