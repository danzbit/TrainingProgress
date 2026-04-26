using AutoMapper;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Domain.Entities;

namespace TrainingService.Application.Builders.v1;

public class WorkoutBuilder
{
    private readonly Workout _workout;

    private readonly bool _isNew;

    public WorkoutBuilder(Guid userId, Guid workoutTypeId, DateTime date)
    {
        _workout = new Workout(userId, workoutTypeId, date);
        _isNew = true;
    }

    public WorkoutBuilder(UpdateWorkoutRequest existingWorkout, IMapper mapper)
    {
        _workout = mapper.Map<Workout>(existingWorkout);
        _isNew = false;
    }

    public WorkoutBuilder WithDuration(int? minutes)
    {
        if (minutes.HasValue)
            _workout.SetDuration(minutes.Value);
        return this;
    }

    public WorkoutBuilder WithNotes(string? notes)
    {
        if (!string.IsNullOrWhiteSpace(notes))
            _workout.SetNotes(notes);
        return this;
    }

    public WorkoutBuilder ApplyExercises(IEnumerable<ExerciseRequest>? exercises)
    {
        if (exercises == null) return this;

        foreach (var ex in exercises)
        {
            if (!_isNew)
            {
                var existing = _workout.Exercises.FirstOrDefault(e => e.Id == ex?.ExerciseId);
                if (existing == null) continue;
                existing.Sets = ex.Sets;
                existing.Reps = ex.Reps;
                existing.WeightKg = ex.WeightKg ?? 0;
            }
            else
            {
                _workout.AddExercise(new Exercise(ex.ExerciseTypeId, ex.Sets, ex.Reps, ex.WeightKg ?? 0,
                    ex.DurationSec ?? 0));
            }
        }

        return this;
    }

    public Workout Build() => _workout;
}

