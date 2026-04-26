// API Client
export { apiClient, createApiClient } from './client';
export { getApiErrorDescription } from './errorUtils';

// Request Functions
export {
  apiGet,
  apiPost,
  apiPut,
  apiPatch,
  apiDelete,
  apiGetPaginated,
  buildEndpoint,
} from './requests';

// Domain APIs
export { workoutApi } from './workoutApi';
export { goalApi } from './goalApi';
export { authApi } from './authApi';
export { trainingTypesApi } from './trainingTypesApi';
export { workoutOrchestrationApi } from './workoutOrchestrationApi';
export { aiChatApi } from './aiChatApi';
export type { ChatMessageDto } from './aiChatApi';
export { goalOrchestrationApi } from './goalOrchestrationApi';
export { analyticsApi } from './analyticsApi';
export { notificationApi } from './notificationApi';
export type { ReminderResponse } from './notificationApi';
export type { SaveGoalPayload, SaveGoalSagaResponse } from './goalOrchestrationApi';
export type { WorkoutSummaryDto, ProfileAnalyticsDto, WorkoutDailyTrendPointDto, WorkoutCountByTypeDto, WorkoutStatisticsOverviewDto } from './analyticsApi';
export type { WorkoutTypeDto, ExerciseTypeDto } from './trainingTypesApi';
export type {
  CreateWorkoutPayload,
  CreateWorkoutExercisePayload,
  CreateWorkoutSagaResponse,
  SagaStepResult,
} from './workoutOrchestrationApi';

// Types
export type {
  ApiResponse,
  ApiErrorResponse,
  ApiErrorWithStatus,
  ApiValidationErrors,
  PaginatedResponse,
  ApiRequestConfig,
  LoginRequest,
  RegisterRequest,
  LoginResponse,
  CurrentUserResponse,
} from './types';
