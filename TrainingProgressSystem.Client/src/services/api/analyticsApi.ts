import apiClient from './client';

const ANALYTICS_ENDPOINT = '/analytics-service/api/v1/workout-analytics';
const PROFILE_ANALYTICS_ENDPOINT = '/analytics-service/api/v1/profile-analytics';

export interface WorkoutSummaryDto {
  amountPerWeek: number;
  weekDurationMin: number;
  amountThisMonth: number;
  monthlyTimeMin: number;
}

export interface ProfileAnalyticsDto {
  totalWorkoutsCompleted: number;
  totalHoursTrained: number;
  goalsAchieved: number;
  workoutsThisWeek: number;
}

export interface WorkoutDailyTrendPointDto {
  date: string;
  workoutsCount: number;
  durationMin: number;
}

export interface WorkoutCountByTypeDto {
  workoutTypeId: string;
  workoutTypeName: string;
  workoutsCount: number;
}

export interface WorkoutStatisticsOverviewDto {
  totalAchievedGoals: number;
  totalTrainingHours: number;
  totalWorkoutsCompleted: number;
  workoutsThisWeek: number;
}

export const analyticsApi = {
  getSummary: async (): Promise<WorkoutSummaryDto> => {
    const response = await apiClient.get<WorkoutSummaryDto>(`${ANALYTICS_ENDPOINT}/summary`);
    return response.data;
  },

  getDailyTrend: async (): Promise<WorkoutDailyTrendPointDto[]> => {
    const response = await apiClient.get<WorkoutDailyTrendPointDto[]>(
      `${ANALYTICS_ENDPOINT}/daily/last-7-days`
    );
    return response.data;
  },

  getCountByType: async (): Promise<WorkoutCountByTypeDto[]> => {
    const response = await apiClient.get<WorkoutCountByTypeDto[]>(
      `${ANALYTICS_ENDPOINT}/by-type`
    );
    return response.data;
  },

  getStatisticsOverview: async (): Promise<WorkoutStatisticsOverviewDto> => {
    const response = await apiClient.get<WorkoutStatisticsOverviewDto>(
      `${ANALYTICS_ENDPOINT}/statistics-overview`
    );
    return response.data;
  },

  getProfileAnalytics: async (): Promise<ProfileAnalyticsDto> => {
    const response = await apiClient.get<ProfileAnalyticsDto>(PROFILE_ANALYTICS_ENDPOINT);
    return response.data;
  },
};
