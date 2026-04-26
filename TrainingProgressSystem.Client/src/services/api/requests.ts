import apiClient from './client';
import type { ApiResponse, ApiErrorWithStatus, PaginatedResponse, ApiRequestConfig } from './types';

/**
 * Generic GET request
 */
export const apiGet = async <T = unknown>(
  endpoint: string,
  config?: ApiRequestConfig
): Promise<T> => {
  try {
    const response = await apiClient.get<ApiResponse<T>>(endpoint, config);
    return response.data.data as T;
  } catch (error) {
    throw handleApiError(error);
  }
};

/**
 * Generic POST request
 */
export const apiPost = async <T = unknown, D = unknown>(
  endpoint: string,
  data?: D,
  config?: ApiRequestConfig
): Promise<T> => {
  try {
    const response = await apiClient.post<ApiResponse<T>>(endpoint, data, config);
    return response.data.data as T;
  } catch (error) {
    throw handleApiError(error);
  }
};

/**
 * Generic PUT request
 */
export const apiPut = async <T = unknown, D = unknown>(
  endpoint: string,
  data?: D,
  config?: ApiRequestConfig
): Promise<T> => {
  try {
    const response = await apiClient.put<ApiResponse<T>>(endpoint, data, config);
    return response.data.data as T;
  } catch (error) {
    throw handleApiError(error);
  }
};

/**
 * Generic PATCH request
 */
export const apiPatch = async <T = unknown, D = unknown>(
  endpoint: string,
  data?: D,
  config?: ApiRequestConfig
): Promise<T> => {
  try {
    const response = await apiClient.patch<ApiResponse<T>>(endpoint, data, config);
    return response.data.data as T;
  } catch (error) {
    throw handleApiError(error);
  }
};

/**
 * Generic DELETE request
 */
export const apiDelete = async <T = unknown>(
  endpoint: string,
  config?: ApiRequestConfig
): Promise<T> => {
  try {
    const response = await apiClient.delete<ApiResponse<T>>(endpoint, config);
    return response.data.data as T;
  } catch (error) {
    throw handleApiError(error);
  }
};

/**
 * GET with pagination support
 */
export const apiGetPaginated = async <T = unknown>(
  endpoint: string,
  pageNumber: number = 1,
  pageSize: number = 10,
  config?: ApiRequestConfig
): Promise<PaginatedResponse<T>> => {
  try {
    const params = {
      pageNumber,
      pageSize,
      ...config?.params,
    };
    const response = await apiClient.get<PaginatedResponse<T>>(endpoint, {
      ...config,
      params,
    });
    return response.data;
  } catch (error) {
    throw handleApiError(error);
  }
};

/**
 * Error handling utility
 */
const handleApiError = (error: unknown): ApiErrorWithStatus => {
  if (error instanceof Error) {
    return error as ApiErrorWithStatus;
  }
  const apiError: ApiErrorWithStatus = new Error('An unexpected error occurred');
  return apiError;
};

/**
 * Helper to build endpoint URL
 */
export const buildEndpoint = (
  base: string,
  ...segments: (string | number)[]
): string => {
  return [base, ...segments].filter(Boolean).join('/');
};
