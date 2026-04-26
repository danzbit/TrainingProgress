import { GoalMetricType, GoalPeriodType, type Workout } from '@/types/workout';
import type { RootState } from './store';

// Auth selectors
export const selectAuth = (state: RootState) => state.auth;
export const selectCurrentUser = (state: RootState) => state.auth.user;
export const selectIsAuthenticated = (state: RootState) => state.auth.isAuthenticated;
export const selectAuthLoading = (state: RootState) => state.auth.isLoading;

export const selectWorkouts = (state: RootState) => state.workout.workouts;
export const selectGoals = (state: RootState) => state.workout.goals;

export const getWorkoutsForDate = (workouts: Workout[], date: Date): Workout[] =>
  workouts.filter((w) => {
    const workoutDate = new Date(w.date);
    return (
      workoutDate.getDate() === date.getDate() &&
      workoutDate.getMonth() === date.getMonth() &&
      workoutDate.getFullYear() === date.getFullYear()
    );
  });

export const getWorkoutsForWeek = (workouts: Workout[], date: Date): Workout[] => {
  const startOfWeek = new Date(date);
  startOfWeek.setDate(date.getDate() - date.getDay());
  startOfWeek.setHours(0, 0, 0, 0);

  const endOfWeek = new Date(startOfWeek);
  endOfWeek.setDate(startOfWeek.getDate() + 6);
  endOfWeek.setHours(23, 59, 59, 999);

  return workouts.filter((w) => {
    const workoutDate = new Date(w.date);
    return workoutDate >= startOfWeek && workoutDate <= endOfWeek;
  });
};

export const getWorkoutsForMonth = (workouts: Workout[], date: Date): Workout[] =>
  workouts.filter((w) => {
    const workoutDate = new Date(w.date);
    return workoutDate.getMonth() === date.getMonth() && workoutDate.getFullYear() === date.getFullYear();
  });

export const selectWeeklyStats = (state: RootState) => {
  const workouts = getWorkoutsForWeek(state.workout.workouts, new Date());
  return {
    workouts: workouts.length,
    duration: workouts.reduce((acc, w) => acc + w.durationMin, 0),
  };
};

export const selectMonthlyStats = (state: RootState) => {
  const workouts = getWorkoutsForMonth(state.workout.workouts, new Date());
  return {
    workouts: workouts.length,
    duration: workouts.reduce((acc, w) => acc + w.durationMin, 0),
  };
};

export const getGoalCurrentValue = (
  metricType: number,
  periodType: number,
  weeklyStats: { workouts: number; duration: number },
  monthlyStats: { workouts: number; duration: number },
): number => {
  const inWeeklyPeriod = periodType === GoalPeriodType.Weekly;

  if (metricType === GoalMetricType.TotalDurationMinutes) {
    return inWeeklyPeriod ? weeklyStats.duration : monthlyStats.duration;
  }

  return inWeeklyPeriod ? weeklyStats.workouts : monthlyStats.workouts;
};
