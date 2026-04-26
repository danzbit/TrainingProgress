import apiClient from './client';
import type { ApiErrorWithStatus } from './types';

const BFF_GOAL_ENDPOINT = '/api/v1/goal-orchestration';
const TRAINING_GOALS_ENDPOINT = '/training-service/api/v1/goals';

export interface SaveGoalPayload {
  name: string;
  description: string;
  metricType: number;
  periodType: number;
  targetValue: number;
  startDate: string;
  endDate?: string;
  correlationId?: string;
}

export interface SaveGoalSagaResponse {
  goalId?: string;
  error?: string;
}

export const goalOrchestrationApi = {
  saveGoal: async (
    payload: SaveGoalPayload,
    idempotencyKey: string,
  ): Promise<SaveGoalSagaResponse> => {
    try {
      const response = await apiClient.post<SaveGoalSagaResponse>(
        BFF_GOAL_ENDPOINT,
        payload,
        { headers: { 'Idempotency-Key': idempotencyKey } },
      );
      return response.data;
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },

  deleteGoal: async (goalId: string): Promise<void> => {
    try {
      await apiClient.delete(`${TRAINING_GOALS_ENDPOINT}/${goalId}`);
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },
};
