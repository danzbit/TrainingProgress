import { apiGet, apiPost, apiPut, apiPatch, apiDelete, buildEndpoint } from './requests';
import type { Goal } from '@/types/workout';

const GOALS_ENDPOINT = '/api/goals';

/**
 * Goal API endpoints
 */
export const goalApi = {
  /**
   * Get all goals
   */
  getGoals: (userId?: string) => {
    const endpoint = userId ? buildEndpoint(GOALS_ENDPOINT, 'user', userId) : GOALS_ENDPOINT;
    return apiGet<Goal[]>(endpoint);
  },

  /**
   * Get goal by ID
   */
  getGoal: (goalId: string) => {
    return apiGet<Goal>(buildEndpoint(GOALS_ENDPOINT, goalId));
  },

  /**
   * Create new goal
   */
  createGoal: (goal: Omit<Goal, 'id'>) => {
    return apiPost<Goal>(GOALS_ENDPOINT, goal);
  },

  /**
   * Update existing goal
   */
  updateGoal: (goalId: string, updates: Partial<Goal>) => {
    return apiPut<Goal>(buildEndpoint(GOALS_ENDPOINT, goalId), updates);
  },

  /**
   * Partially update goal
   */
  patchGoal: (goalId: string, updates: Partial<Goal>) => {
    return apiPatch<Goal>(buildEndpoint(GOALS_ENDPOINT, goalId), updates);
  },

  /**
   * Delete goal
   */
  deleteGoal: (goalId: string) => {
    return apiDelete<void>(buildEndpoint(GOALS_ENDPOINT, goalId));
  },

  /**
   * Update goal progress
   */
  updateGoalProgress: (goalId: string, currentValue: number) => {
    return apiPatch<Goal>(buildEndpoint(GOALS_ENDPOINT, goalId, 'progress'), {
      currentValue,
    });
  },

  /**
   * Get goals for user
   */
  getUserGoals: (userId: string) => {
    return apiGet<Goal[]>(buildEndpoint(GOALS_ENDPOINT, 'user', userId));
  },
};
