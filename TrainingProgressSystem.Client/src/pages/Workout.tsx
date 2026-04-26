import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '@/components/Layout';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useAppSelector } from '@/store/hooks';
import { selectCurrentUser } from '@/store/selectors';
import { type Exercise, WORKOUT_TYPES } from '@/types/workout';
import { getApiErrorDescription, trainingTypesApi, workoutOrchestrationApi } from '@/services/api';
import type { ExerciseTypeDto } from '@/services/api';
import { Plus, Trash2, Save, Dumbbell } from 'lucide-react';
import { toast } from '@/hooks/use-toast';

/** Which exercise categories are relevant for each workout type label. */
const WORKOUT_TYPE_CATEGORY_MAP: Record<string, string[]> = {
  Strength:    ['Strength', 'Core'],
  Cardio:      ['Cardio', 'Core'],
  Flexibility: ['Flexibility', 'Core'],
  HIIT:        ['Strength', 'Cardio', 'Core'],
  Yoga:        ['Flexibility', 'Core'],
  Sports:      ['Strength', 'Cardio', 'Flexibility', 'Core'],
  Other:       ['Strength', 'Cardio', 'Flexibility', 'Core'],
};

const WorkoutPage = () => {
  const navigate = useNavigate();
  const currentUser = useAppSelector(selectCurrentUser);

  const [date, setDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [workoutTypeId, setWorkoutTypeId] = useState<string>(WORKOUT_TYPES[0].value);
  const [durationMin, setDurationMin] = useState<string>('30');
  const [notes, setNotes] = useState<string>('');
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [exerciseTypes, setExerciseTypes] = useState<ExerciseTypeDto[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    trainingTypesApi.getExerciseTypes()
      .then(setExerciseTypes)
      .catch(() => {
        // Non-critical — exercises remain optional
      });
  }, []);

  const selectedWorkoutTypeLabel =
    WORKOUT_TYPES.find((t) => t.value === workoutTypeId)?.label ?? '';
  const allowedCategories = WORKOUT_TYPE_CATEGORY_MAP[selectedWorkoutTypeLabel] ?? null;
  const filteredExerciseTypes = allowedCategories
    ? exerciseTypes.filter((et) => allowedCategories.includes(et.category))
    : exerciseTypes;

  const addExercise = () => {
    const newExercise: Exercise = {
      id: Math.random().toString(36).substring(2, 15),
      name: '',
      exerciseTypeId: '',
      sets: 3,
      reps: 10,
      weight: undefined,
    };
    setExercises([...exercises, newExercise]);
  };

  const updateExercise = (id: string, updates: Partial<Exercise>) => {
    setExercises(
      exercises.map((ex) => (ex.id === id ? { ...ex, ...updates } : ex))
    );
  };

  const removeExercise = (id: string) => {
    setExercises(exercises.filter((ex) => ex.id !== id));
  };

  const handleSubmit = async () => {
    if (!currentUser) {
      toast({ title: 'Not authenticated', description: 'Please sign in to log a workout.', variant: 'destructive' });
      return;
    }

    if (!durationMin || parseInt(durationMin) <= 0) {
      toast({
        title: 'Invalid duration',
        description: 'Please enter a valid workout duration.',
        variant: 'destructive',
      });
      return;
    }

    const validExercises = exercises.filter((ex) => ex.exerciseTypeId.trim() !== '');

    setIsSubmitting(true);
    try {
      const idempotencyKey = crypto.randomUUID();
      await workoutOrchestrationApi.createWorkout(
        {
          date: new Date(date).toISOString(),
          workoutTypeId,
          durationMin: parseInt(durationMin),
          notes: notes || undefined,
          exercises: validExercises.map((ex) => ({
            exerciseTypeId: ex.exerciseTypeId,
            sets: ex.sets ?? 1,
            reps: ex.reps ?? 1,
            durationSec: ex.duration ? ex.duration * 60 : undefined,
            weightKg: ex.weight,
          })),
        },
        idempotencyKey,
      );

      toast({
        title: 'Workout logged! 💪',
        description: 'Your workout has been saved successfully.',
      });
      navigate('/history');
    } catch (err: unknown) {
      const description = getApiErrorDescription(err, 'Could not reach the server. Please try again.');
      toast({
        title: 'Failed to save workout',
        description,
        variant: 'destructive',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Layout>
      <div className="py-6 max-w-2xl mx-auto space-y-8">
        {/* Header */}
        <div className="text-center space-y-2">
          <div className="w-16 h-16 mx-auto rounded-2xl gradient-primary flex items-center justify-center mb-4">
            <Dumbbell className="w-8 h-8 text-primary-foreground" />
          </div>
          <h1 className="text-2xl font-bold text-foreground">Log Workout</h1>
          <p className="text-muted-foreground">
            Record your training session and track your progress
          </p>
        </div>

        {/* Form */}
        <div className="space-y-6">
          {/* Date and Type Row */}
          <div className="grid md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Date</Label>
              <Input
                type="date"
                value={date}
                onChange={(e) => setDate(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label>Workout Type</Label>
              <Select value={workoutTypeId} onValueChange={(v) => {
                setWorkoutTypeId(v);
                // Clear exercise selections that may no longer be valid for the new type
                setExercises((prev) => prev.map((ex) => ({ ...ex, exerciseTypeId: '', name: '' })));
              }}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {WORKOUT_TYPES.map((t) => (
                    <SelectItem key={t.value} value={t.value}>
                      <span className="flex items-center gap-2">
                        <span>{t.icon}</span>
                        <span>{t.label}</span>
                      </span>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Duration */}
          <div className="space-y-2">
            <Label>Duration (minutes)</Label>
            <Input
              type="number"
              value={durationMin}
              onChange={(e) => setDurationMin(e.target.value)}
              placeholder="30"
              min={1}
            />
          </div>

          {/* Description */}
          <div className="space-y-2">
            <Label>Notes</Label>
            <Textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="What did you focus on today? Any notes about your workout..."
              rows={3}
            />
          </div>

          {/* Exercises */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label>Exercises (Optional)</Label>
              <Button variant="outline" size="sm" onClick={addExercise}>
                <Plus className="w-4 h-4 mr-1" />
                Add Exercise
              </Button>
            </div>

            {exercises.length > 0 ? (
              <div className="space-y-3">
                {exercises.map((exercise, index) => (
                  <div
                    key={exercise.id}
                    className="p-4 rounded-xl bg-muted/50 border border-border space-y-3"
                  >
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium text-muted-foreground">
                        Exercise {index + 1}
                      </span>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8 text-muted-foreground hover:text-destructive"
                        onClick={() => removeExercise(exercise.id)}
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>

                    {filteredExerciseTypes.length > 0 ? (
                      <Select
                        value={exercise.exerciseTypeId}
                        onValueChange={(value) => {
                          const et = filteredExerciseTypes.find((t) => t.id === value);
                          updateExercise(exercise.id, {
                            exerciseTypeId: value,
                            name: et?.name ?? '',
                          });
                        }}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Select exercise type" />
                        </SelectTrigger>
                        <SelectContent>
                          {filteredExerciseTypes.map((et) => (
                            <SelectItem key={et.id} value={et.id}>
                              <span className="flex items-center gap-2">
                                <span className="text-xs text-muted-foreground">{et.category}</span>
                                <span>{et.name}</span>
                              </span>
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    ) : (
                      <Input
                        placeholder="Exercise name (e.g., Bench Press)"
                        value={exercise.name}
                        onChange={(e) =>
                          updateExercise(exercise.id, { name: e.target.value, exerciseTypeId: '' })
                        }
                      />
                    )}

                    <div className="grid grid-cols-3 gap-3">
                      <div className="space-y-1">
                        <Label className="text-xs">Sets</Label>
                        <Input
                          type="number"
                          value={exercise.sets || ''}
                          onChange={(e) =>
                            updateExercise(exercise.id, {
                              sets: parseInt(e.target.value) || undefined,
                            })
                          }
                          placeholder="3"
                        />
                      </div>
                      <div className="space-y-1">
                        <Label className="text-xs">Reps</Label>
                        <Input
                          type="number"
                          value={exercise.reps || ''}
                          onChange={(e) =>
                            updateExercise(exercise.id, {
                              reps: parseInt(e.target.value) || undefined,
                            })
                          }
                          placeholder="10"
                        />
                      </div>
                      <div className="space-y-1">
                        <Label className="text-xs">Weight (kg)</Label>
                        <Input
                          type="number"
                          value={exercise.weight || ''}
                          onChange={(e) =>
                            updateExercise(exercise.id, {
                              weight: parseFloat(e.target.value) || undefined,
                            })
                          }
                          placeholder="20"
                        />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground text-center py-4 border border-dashed border-border rounded-xl">
                No exercises added yet. Click "Add Exercise" to track specific movements.
              </p>
            )}
          </div>

          {/* Submit */}
          <Button onClick={handleSubmit} className="w-full" size="lg" disabled={isSubmitting}>
            <Save className="w-4 h-4 mr-2" />
            {isSubmitting ? 'Saving...' : 'Save Workout'}
          </Button>
        </div>
      </div>
    </Layout>
  );
};

export default WorkoutPage;
