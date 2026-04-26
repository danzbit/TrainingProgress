import { type Workout, type WorkoutsListItemDto, WORKOUT_TYPES } from '@/types/workout';
import { format } from 'date-fns';
import { Clock, MoreVertical, Trash2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Button } from '@/components/ui/button';

type WorkoutCardItem = Workout | WorkoutsListItemDto;

interface WorkoutCardProps {
  workout: WorkoutCardItem;
  onDelete?: (id: string) => void;
  compact?: boolean;
}

function isListItem(w: WorkoutCardItem): w is WorkoutsListItemDto {
  return 'workoutType' in w && !('workoutTypeId' in w);
}

export function WorkoutCard({ workout, onDelete, compact = false }: WorkoutCardProps) {
  const workoutType = isListItem(workout)
    ? WORKOUT_TYPES.find((t) => t.label === workout.workoutType)
    : WORKOUT_TYPES.find((t) => t.value === workout.workoutTypeId);

  const label = isListItem(workout) ? workout.workoutType : (workoutType?.label ?? 'Workout');
  const date = workout.date;
  const durationMin = workout.durationMin;
  const exerciseCount = isListItem(workout) ? workout.amountOfExercises : workout.exercises.length;
  const notes = isListItem(workout) ? undefined : workout.notes;
  const id = isListItem(workout) ? undefined : workout.id;

  return (
    <div className={cn('workout-card', compact && 'p-3')}>
      <div className="flex items-start justify-between">
        <div className="flex items-start gap-3">
          <div className="w-10 h-10 rounded-lg bg-accent flex items-center justify-center text-xl">
            {workoutType?.icon || '🎯'}
          </div>
          <div className="space-y-1">
            <div className="flex items-center gap-2">
              <h4 className="font-semibold text-foreground">
                {label}
              </h4>
              <span className="text-xs text-muted-foreground px-2 py-0.5 rounded-full bg-muted">
                {format(new Date(date), 'MMM d')}
              </span>
            </div>
            {!compact && notes !== undefined && (
              <p className="text-sm text-muted-foreground line-clamp-2">
                {notes || 'No notes'}
              </p>
            )}
            <div className="flex items-center gap-1 text-sm text-muted-foreground">
              <Clock className="w-3.5 h-3.5" />
              <span>{durationMin} min</span>
              {exerciseCount > 0 && (
                <>
                  <span className="mx-1">•</span>
                  <span>{exerciseCount} exercises</span>
                </>
              )}
            </div>
          </div>
        </div>

        {onDelete && id && (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreVertical className="w-4 h-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem
                onClick={() => onDelete(id)}
                className="text-destructive focus:text-destructive"
              >
                <Trash2 className="w-4 h-4 mr-2" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </div>
  );
}
