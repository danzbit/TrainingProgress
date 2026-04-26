import { apiGet, apiPost, apiPut, apiPatch, apiDelete, buildEndpoint } from './requests';
import apiClient from './client';
import type { Workout, WorkoutsListItemDto, GoalsListItemDto } from '@/types/workout';

export interface ODataPage<T> {
  items: T[];
  totalCount: number;
  /** true when server returned @odata.count, false when estimated from page size */
  countIsExact: boolean;
}

const WORKOUTS_ENDPOINT = '/training-service/api/v1/workouts';
const GOALS_ENDPOINT = '/training-service/api/v1/goals';

/**
 * Workout API endpoints
 */
export const workoutApi = {
  /**
   * Get a page of workouts via OData with server-side pagination.
   * Returns { items, totalCount } from OData $count envelope.
   */
  getAllWorkouts: async (odataQuery?: string, signal?: AbortSignal): Promise<ODataPage<WorkoutsListItemDto>> => {
    const url = odataQuery ? `${WORKOUTS_ENDPOINT}?${odataQuery}` : WORKOUTS_ENDPOINT;
    const response = await apiClient.get<unknown>(url, { signal });
    const data = response.data as any;
    if (Array.isArray(data)) {
      return { items: data as WorkoutsListItemDto[], totalCount: data.length, countIsExact: false };
    }
    const items: WorkoutsListItemDto[] = data?.value ?? [];
    const rawCount = data?.['@odata.count'];
    const countIsExact = rawCount !== undefined && rawCount !== null;
    const totalCount: number = countIsExact ? Number(rawCount) : items.length;
    return { items, totalCount, countIsExact };
  },

  /**
   * Get a single page of workouts with server-side pagination.
   */
  getWorkoutsPage: async (page: number, pageSize: number, signal?: AbortSignal): Promise<ODataPage<WorkoutsListItemDto>> => {
    const skip = page * pageSize;
    const query = `$orderby=date desc&$top=${pageSize}&$skip=${skip}&$count=true`;
    return workoutApi.getAllWorkouts(query, signal);
  },

  /**
   * Get total count of workouts using a lightweight $top=0&$count=true request.
   * Returns -1 if the server does not return @odata.count.
   */
  getWorkoutCount: async (signal?: AbortSignal): Promise<number> => {
    const url = `${WORKOUTS_ENDPOINT}?$select=date`;
    const response = await apiClient.get<unknown>(url, { signal });
    const data = response.data as any;
    if (Array.isArray(data)) return data.length;
    return (data?.value ?? data?.['@odata.count'] !== undefined)
      ? (data?.['@odata.count'] ?? (data?.value as any[])?.length ?? 0)
      : 0;
  },

  getHistoryViewPreference: async (signal?: AbortSignal): Promise<'list' | 'calendar'> => {
    const response = await apiClient.get<{ historyViewMode: string }>('/training-service/api/v1/userpreferences', { signal });
    const mode = response.data?.historyViewMode;
    return mode === 'calendar' ? 'calendar' : 'list';
  },

  saveHistoryViewPreference: async (viewMode: 'list' | 'calendar'): Promise<void> => {
    await apiClient.put('/training-service/api/v1/userpreferences', { historyViewMode: viewMode });
  },

  /**
   * Get all workouts
   */
  getWorkouts: (userId?: string) => {
    const endpoint = userId ? buildEndpoint(WORKOUTS_ENDPOINT, 'user', userId) : WORKOUTS_ENDPOINT;
    return apiGet<Workout[]>(endpoint);
  },

  /**
   * Get workout by ID
   */
  getWorkout: (workoutId: string) => {
    return apiGet<Workout>(buildEndpoint(WORKOUTS_ENDPOINT, workoutId));
  },

  /**
   * Create new workout
   */
  createWorkout: (workout: Omit<Workout, 'id' | 'createdAt'>) => {
    return apiPost<Workout>(WORKOUTS_ENDPOINT, workout);
  },

  /**
   * Update existing workout
   */
  updateWorkout: (workoutId: string, updates: Partial<Workout>) => {
    return apiPut<Workout>(buildEndpoint(WORKOUTS_ENDPOINT, workoutId), updates);
  },

  /**
   * Partially update workout
   */
  patchWorkout: (workoutId: string, updates: Partial<Workout>) => {
    return apiPatch<Workout>(buildEndpoint(WORKOUTS_ENDPOINT, workoutId), updates);
  },

  /**
   * Delete workout
   */
  deleteWorkout: (workoutId: string) => {
    return apiDelete<void>(buildEndpoint(WORKOUTS_ENDPOINT, workoutId));
  },

  /**
   * Get workouts for date range
   */
  getWorkoutsForDateRange: (userId: string, startDate: string, endDate: string) => {
    return apiGet<Workout[]>(
      buildEndpoint(WORKOUTS_ENDPOINT, 'user', userId, 'range'),
      {
        params: { startDate, endDate },
      }
    );
  },

  /**
   * Get workouts for a specific date via OData $filter.
   */
  getWorkoutsByDate: async (date: Date, signal?: AbortSignal): Promise<WorkoutsListItemDto[]> => {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    const dateStr = `${yyyy}-${mm}-${dd}`;
    const filter = `$filter=date eq ${dateStr}`;
    const url = `${WORKOUTS_ENDPOINT}?${filter}`;
    const response = await apiClient.get<unknown>(url, { signal });
    const data = response.data as any;
    return Array.isArray(data) ? data : data?.value ?? [];
  },

  /**
   * Get a page of goals via OData. Returns { items, totalCount }.
   */
  getAllGoals: async (signal?: AbortSignal): Promise<ODataPage<GoalsListItemDto>> => {
    const response = await apiClient.get<GoalsListItemDto[]>(GOALS_ENDPOINT, { signal });
    const items = Array.isArray(response.data) ? response.data : (response.data as any)?.value ?? [];
    return { items, totalCount: items.length, countIsExact: false };
  },
};
