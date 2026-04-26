import type { Exercise } from './exercise';

export interface WorkoutTypeRef {
  id: string;
  name: string;
  description?: string;
}

export interface WorkoutTypeOption {
  value: string;
  label: string;
  icon: string;
}

export interface Workout {
  id: string;
  userId: string;
  date: Date;
  workoutTypeId: string;
  durationMin: number;
  notes?: string;
  createdAt?: Date;
  workoutType?: WorkoutTypeRef;
  exercises: Exercise[];
}

/** Shape returned by GET /api/v1/workouts (OData list endpoint) */
export interface WorkoutsListItemDto {
  workoutType: string;
  durationMin: number;
  amountOfExercises: number;
  date: string;
}

export const CLIENT_DEMO_USER_ID = '11111111-1111-1111-1111-111111111111';

// Replace these ids with values loaded from backend WorkoutTypes when API integration is added.
export const WORKOUT_TYPES: WorkoutTypeOption[] = [
  { value: 'a1111111-1111-1111-1111-111111111111', label: 'Strength', icon: '💪' },
  { value: 'a2222222-2222-2222-2222-222222222222', label: 'Cardio', icon: '🏃' },
  { value: 'a3333333-3333-3333-3333-333333333333', label: 'Flexibility', icon: '🧘' },
  { value: 'a4444444-4444-4444-4444-444444444444', label: 'HIIT', icon: '⚡' },
  { value: 'a5555555-5555-5555-5555-555555555555', label: 'Yoga', icon: '🧘‍♀️' },
  { value: 'a6666666-6666-6666-6666-666666666666', label: 'Sports', icon: '⚽' },
  { value: 'a7777777-7777-7777-7777-777777777777', label: 'Other', icon: '🎯' },
];
