export const GoalMetricType = {
  WorkoutCount: 0,
  TotalDurationMinutes: 1,
  DistanceKm: 2,
  CaloriesBurned: 3,
  StreakDays: 4,
  UniqueWorkoutTypes: 5,
  WeekendWorkouts: 6,
  MorningWorkouts: 7,
} as const;

export type GoalMetricType = (typeof GoalMetricType)[keyof typeof GoalMetricType];

export const GoalPeriodType = {
  Weekly: 0,
  Monthly: 1,
  CustomRange: 2,
  RollingWindow: 3,
} as const;

export type GoalPeriodType = (typeof GoalPeriodType)[keyof typeof GoalPeriodType];

export const GoalStatus = {
  Active: 0,
  Completed: 1,
  Cancelled: 2,
} as const;

export type GoalStatus = (typeof GoalStatus)[keyof typeof GoalStatus];

export interface Goal {
  id: string;
  userId: string;
  name: string;
  description: string;
  metricType: GoalMetricType;
  periodType: GoalPeriodType;
  targetValue: number;
  status: GoalStatus;
  startDate: Date;
  endDate?: Date;
  currentValue: number;
  progressPercentage: number;
  isCompleted: boolean;
  lastCalculatedAt?: Date;
}

export const GOAL_METRIC_OPTIONS = [
  { value: GoalMetricType.WorkoutCount, label: 'Workouts', unit: 'workouts' },
  { value: GoalMetricType.TotalDurationMinutes, label: 'Total Minutes', unit: 'minutes' },
  { value: GoalMetricType.DistanceKm, label: 'Distance (km)', unit: 'km' },
  { value: GoalMetricType.CaloriesBurned, label: 'Calories Burned', unit: 'kcal' },
  { value: GoalMetricType.StreakDays, label: 'Streak Days', unit: 'days' },
  { value: GoalMetricType.UniqueWorkoutTypes, label: 'Unique Workout Types', unit: 'types' },
  { value: GoalMetricType.WeekendWorkouts, label: 'Weekend Workouts', unit: 'workouts' },
  { value: GoalMetricType.MorningWorkouts, label: 'Morning Workouts', unit: 'workouts' },
] as const;

export const GOAL_PERIOD_OPTIONS = [
  { value: GoalPeriodType.Weekly, label: 'Weekly' },
  { value: GoalPeriodType.Monthly, label: 'Monthly' },
  { value: GoalPeriodType.CustomRange, label: 'Custom Range' },
  { value: GoalPeriodType.RollingWindow, label: 'Rolling Window (30 days)' },
] as const;

export const getGoalUnit = (metricType: GoalMetricType): string => {
  switch (metricType) {
    case GoalMetricType.TotalDurationMinutes:
      return 'minutes';
    case GoalMetricType.WorkoutCount:
      return 'workouts';
    case GoalMetricType.DistanceKm:
      return 'km';
    case GoalMetricType.CaloriesBurned:
      return 'kcal';
    case GoalMetricType.StreakDays:
      return 'days';
    case GoalMetricType.UniqueWorkoutTypes:
      return 'types';
    case GoalMetricType.WeekendWorkouts:
      return 'weekend workouts';
    case GoalMetricType.MorningWorkouts:
      return 'morning workouts';
    default:
      return 'units';
  }
};

export const isGoalCompleted = (goal: Goal): boolean =>
  goal.status === GoalStatus.Completed || goal.currentValue >= goal.targetValue;

/** Shape returned by GET /training-service/api/v1/goals (OData list endpoint) */
export interface GoalProgressInfo {
  currentValue: number;
  progressPercentage: number;
  isCompleted: boolean;
  lastCalculatedAt?: string;
}

export interface GoalsListItemDto {
  id: string;
  name: string;
  description: string;
  metricType: GoalMetricType;
  periodType: GoalPeriodType;
  targetValue: number;
  status: GoalStatus;
  startDate: string;
  endDate?: string;
  progressInfo: GoalProgressInfo;
}
