import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import {
  CLIENT_DEMO_USER_ID,
  GoalMetricType,
  GoalPeriodType,
  GoalStatus,
  WORKOUT_TYPES,
  type Goal,
  type Workout,
} from '@/types/workout';

const STORAGE_KEY = 'lovable-fitness-storage';

export interface WorkoutState {
  workouts: Workout[];
  goals: Goal[];
}

export type CreateWorkoutInput = Omit<Workout, 'id' | 'createdAt' | 'workoutType'>;
export type CreateGoalInput = Omit<Goal, 'id' | 'status' | 'currentValue' | 'progressPercentage' | 'isCompleted' | 'lastCalculatedAt'>;

const generateId = () => Math.random().toString(36).substring(2, 15);

const toDate = (value: unknown): Date => {
  if (value instanceof Date) return value;
  if (typeof value === 'string' || typeof value === 'number') return new Date(value);
  return new Date();
};

const toWorkoutTypeId = (value: unknown): string => {
  if (typeof value === 'string' && value) {
    const byLabel = WORKOUT_TYPES.find((w) => w.label.toLowerCase() === value.toLowerCase());
    return byLabel?.value || value;
  }
  return WORKOUT_TYPES[0].value;
};

const defaultState: WorkoutState = {
  workouts: [],
  goals: [],
};

const migrateState = (persistedState: unknown): WorkoutState => {
  const state = persistedState as { workouts?: unknown[]; goals?: unknown[] };

  const migratedWorkouts = (state.workouts || []).map((w) => {
    const workout = w as Record<string, unknown>;
    const workoutTypeId = toWorkoutTypeId(workout.workoutTypeId ?? workout.type);
    const workoutType = WORKOUT_TYPES.find((x) => x.value === workoutTypeId);

    return {
      id: String(workout.id || generateId()),
      userId: String(workout.userId || CLIENT_DEMO_USER_ID),
      date: toDate(workout.date),
      workoutTypeId,
      durationMin: Number(workout.durationMin ?? workout.duration ?? 0),
      notes: String(workout.notes ?? workout.description ?? ''),
      createdAt: workout.createdAt ? toDate(workout.createdAt) : new Date(),
      exercises: Array.isArray(workout.exercises) ? (workout.exercises as Workout['exercises']) : [],
      workoutType: workoutType
        ? {
            id: workoutType.value,
            name: workoutType.label,
            description: workoutType.label,
          }
        : undefined,
    } satisfies Workout;
  });

  const migratedGoals = (state.goals || []).map((g) => {
    const goal = g as Record<string, unknown>;
    const metricType: Goal['metricType'] =
      goal.metricType !== undefined
        ? (Number(goal.metricType) as Goal['metricType'])
        : String(goal.unit || '').toLowerCase() === 'minutes'
        ? GoalMetricType.TotalDurationMinutes
        : GoalMetricType.WorkoutCount;
    const periodType: Goal['periodType'] =
      goal.periodType !== undefined
        ? (Number(goal.periodType) as Goal['periodType'])
        : String(goal.period || '').toLowerCase() === 'monthly'
        ? GoalPeriodType.Monthly
        : GoalPeriodType.Weekly;

    const targetValue = Number(goal.targetValue ?? goal.target ?? 0);
    const currentValue = Number(goal.currentValue ?? goal.current ?? 0);
    const isCompleted = Boolean(goal.isCompleted ?? currentValue >= targetValue);

    return {
      id: String(goal.id || generateId()),
      userId: String(goal.userId || CLIENT_DEMO_USER_ID),
      name: String(goal.name ?? goal.title ?? 'Goal'),
      description: String(goal.description ?? ''),
      metricType,
      periodType,
      targetValue,
      status: Number(goal.status ?? (isCompleted ? GoalStatus.Completed : GoalStatus.Active)) as Goal['status'],
      startDate: toDate(goal.startDate ?? goal.createdAt),
      endDate: goal.endDate ? toDate(goal.endDate) : undefined,
      currentValue,
      progressPercentage:
        goal.progressPercentage !== undefined
          ? Number(goal.progressPercentage)
          : targetValue > 0
          ? Math.min((currentValue / targetValue) * 100, 100)
          : 0,
      isCompleted,
      lastCalculatedAt: goal.lastCalculatedAt ? toDate(goal.lastCalculatedAt) : new Date(),
    } satisfies Goal;
  });

  return {
    workouts: migratedWorkouts,
    goals: migratedGoals,
  };
};

const STORAGE_VERSION = 3;

export const loadWorkoutState = (): WorkoutState => {
  if (typeof window === 'undefined') return defaultState;

  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return defaultState;

    const parsed = JSON.parse(raw) as unknown;
    if (!parsed || typeof parsed !== 'object') return defaultState;

    const record = parsed as Record<string, unknown>;
    const version = typeof record.version === 'number' ? record.version : 0;
    const candidate = 'state' in record ? (record as { state: unknown }).state : parsed;

    const migrated = migrateState(candidate);

    // Versions below 3 contained hardcoded mock goals — drop them
    if (version < STORAGE_VERSION) {
      return { ...migrated, goals: [] };
    }

    return migrated;
  } catch {
    return defaultState;
  }
};

export const saveWorkoutState = (state: WorkoutState): void => {
  if (typeof window === 'undefined') return;

  localStorage.setItem(
    STORAGE_KEY,
    JSON.stringify({
      version: STORAGE_VERSION,
      state,
    }),
  );
};

const workoutSlice = createSlice({
  name: 'workout',
  initialState: loadWorkoutState(),
  reducers: {
    addWorkout: (state, action: PayloadAction<CreateWorkoutInput>) => {
      const workout = action.payload;
      const workoutType = WORKOUT_TYPES.find((w) => w.value === workout.workoutTypeId);

      state.workouts.push({
        ...workout,
        id: generateId(),
        userId: workout.userId || CLIENT_DEMO_USER_ID,
        createdAt: new Date(),
        workoutType: workoutType
          ? {
              id: workoutType.value,
              name: workoutType.label,
              description: workoutType.label,
            }
          : undefined,
      });
    },
    removeWorkout: (state, action: PayloadAction<string>) => {
      state.workouts = state.workouts.filter((w) => w.id !== action.payload);
    },
    updateWorkout: (state, action: PayloadAction<{ id: string; updates: Partial<Workout> }>) => {
      const { id, updates } = action.payload;
      state.workouts = state.workouts.map((w) => (w.id === id ? { ...w, ...updates } : w));
    },
    addGoal: (state, action: PayloadAction<CreateGoalInput>) => {
      const goal = action.payload;
      state.goals.push({
        ...goal,
        id: generateId(),
        userId: goal.userId || CLIENT_DEMO_USER_ID,
        status: GoalStatus.Active,
        currentValue: 0,
        progressPercentage: 0,
        isCompleted: false,
        lastCalculatedAt: new Date(),
      });
    },
    removeGoal: (state, action: PayloadAction<string>) => {
      state.goals = state.goals.filter((g) => g.id !== action.payload);
    },
    updateGoalProgress: (state, action: PayloadAction<{ id: string; currentValue: number }>) => {
      const { id, currentValue } = action.payload;
      state.goals = state.goals.map((g) => {
        if (g.id !== id) return g;

        const completed = currentValue >= g.targetValue;
        return {
          ...g,
          currentValue,
          progressPercentage: Math.min((currentValue / g.targetValue) * 100, 100),
          isCompleted: completed,
          status: completed ? GoalStatus.Completed : g.status,
          lastCalculatedAt: new Date(),
        };
      });
    },
  },
});

export const { addWorkout, removeWorkout, updateWorkout, addGoal, removeGoal, updateGoalProgress } =
  workoutSlice.actions;

export default workoutSlice.reducer;
