using Shared.Kernal.Models;

namespace TrainingService.Domain.Entities;

public class Exercise(Guid exerciseTypeId, int sets, int reps, decimal? weightKg = null, int? durationSec = null) : BaseEntity
{
    public Guid WorkoutId { get; set; }

    public Guid ExerciseTypeId { get; set; } = exerciseTypeId;

    public int Sets { get; set; } = sets;

    public int Reps { get; set; } = reps;

    public decimal? WeightKg { get; set; } = weightKg;

    public int? DurationSec { get; set; } = durationSec;

    public Workout Workout { get; set; } = null!;

    public ExerciseType ExerciseType { get; set; } = null!;
}