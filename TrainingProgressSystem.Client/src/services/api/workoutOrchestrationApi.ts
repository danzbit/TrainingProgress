import apiClient from './client';
import type { ApiErrorWithStatus } from './types';

const BFF_WORKOUT_ENDPOINT = '/api/v1/workout-orchestration';

export interface CreateWorkoutExercisePayload {
  /** Optional reference to an existing Exercise entity */
  exerciseId?: string;
  /** Required — references an ExerciseType entity */
  exerciseTypeId: string;
  sets: number;
  reps: number;
  durationSec?: number;
  weightKg?: number;
}

export interface CreateWorkoutPayload {
  date: string; // ISO-8601
  workoutTypeId: string;
  durationMin?: number;
  notes?: string;
  exercises?: CreateWorkoutExercisePayload[];
  correlationId?: string;
}

export interface SagaStepResult {
  name: string;
  status: number;
  required: boolean;
  error?: string;
}

export interface CreateWorkoutSagaResponse {
  workoutId?: string;
  error?: string;
}

export const workoutOrchestrationApi = {
  createWorkout: async (
    payload: CreateWorkoutPayload,
    idempotencyKey: string,
  ): Promise<CreateWorkoutSagaResponse> => {
    try {
      const response = await apiClient.post<CreateWorkoutSagaResponse>(
        BFF_WORKOUT_ENDPOINT,
        payload,
        { headers: { 'Idempotency-Key': idempotencyKey } },
      );
      return response.data;
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },
};
