import apiClient from './client';
import type { ApiErrorWithStatus } from './types';

export interface WorkoutTypeDto {
  id: string;
  name: string;
  description?: string;
}

export interface ExerciseTypeDto {
  id: string;
  name: string;
  category: string;
}

const TRAINING_ENDPOINT = '/training-service/api/v1';

export const trainingTypesApi = {
  getWorkoutTypes: async (): Promise<WorkoutTypeDto[]> => {
    try {
      const response = await apiClient.get<WorkoutTypeDto[]>(`${TRAINING_ENDPOINT}/workout-types`);
      return response.data;
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },

  getExerciseTypes: async (): Promise<ExerciseTypeDto[]> => {
    try {
      const response = await apiClient.get<ExerciseTypeDto[]>(`${TRAINING_ENDPOINT}/exercise-types`);
      return response.data;
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },
};
