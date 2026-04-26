using Shared.Infrastructure.Identity;
using Shared.Kernal.Models;

namespace TrainingService.Domain.Entities;

public class Workout(Guid userId, Guid workoutTypeId, DateTime date) : BaseEntity
{
    public Guid WorkoutTypeId { get; set; } = workoutTypeId;

    public DateTime Date { get; set; } = date;

    public int DurationMin { get; set; }
    
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; } = userId;

    public ApplicationUser User { get; set; } = null!;
    
    public WorkoutType WorkoutType { get; set; } = null!;

    public ICollection<Exercise> Exercises { get; set; } = [];

    public void SetDuration(int minutes) => DurationMin = minutes;

    public void SetNotes(string notes) => Notes = notes;

    public void AddExercise(Exercise exercise) => Exercises.Add(exercise);
}