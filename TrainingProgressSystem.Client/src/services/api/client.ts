import axios, { AxiosError } from 'axios';
import type { AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import type { ApiErrorWithStatus, ApiErrorResponse } from './types';

type ProblemDetailsPayload = Partial<ApiErrorResponse> & {
  title?: string;
  detail?: string;
  error?: string;
  errors?: Record<string, string[] | string>;
};

const normalizeValidationErrors = (
  errors?: Record<string, string[] | string>,
): Record<string, string[]> | undefined => {
  if (!errors) {
    return undefined;
  }

  const normalizedEntries = Object.entries(errors)
    .map(([key, value]) => {
      const messages = Array.isArray(value)
        ? value.filter((item): item is string => typeof item === 'string' && item.trim().length > 0)
        : typeof value === 'string' && value.trim().length > 0
          ? [value]
          : [];

      return [key, messages] as const;
    })
    .filter(([, messages]) => messages.length > 0);

  if (normalizedEntries.length === 0) {
    return undefined;
  }

  return Object.fromEntries(normalizedEntries);
};

// ── Refresh-token queue ───────────────────────────────────────────────────

type QueueEntry = { resolve: () => void; reject: (e: unknown) => void };

let isRefreshing = false;
let failedQueue: QueueEntry[] = [];

const REFRESH_ENDPOINT = '/auth-service/api/v1/auth/refresh-token';

const NON_REFRESHABLE_PATHS = [
  '/auth/refresh-token',
  '/auth/sign-in',
  '/auth/sign-up',
  '/auth/sign-out',
];

const isNonRefreshable = (url = ''): boolean =>
  NON_REFRESHABLE_PATHS.some((p) => url.includes(p));

const processQueue = (error: unknown): void => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve();
  });
  failedQueue = [];
};

// ── Client factory ────────────────────────────────────────────────────────

export const createApiClient = (): AxiosInstance => {
  const baseURL = import.meta.env.VITE_API_BASE_URL || '';

  const client = axios.create({
    baseURL,
    timeout: 30000,
    headers: { 'Content-Type': 'application/json' },
    withCredentials: true, // send HTTP-only refreshToken cookie
  });

  // ── Response interceptor ─────────────────────────────────────────────────
  client.interceptors.response.use(
    (response) => response,
    async (error: AxiosError<ApiErrorResponse>) => {
      const originalRequest = error.config as InternalAxiosRequestConfig & {
        _retry?: boolean;
      };

      const responseData = error.response?.data as ProblemDetailsPayload | undefined;
      const validationErrors = normalizeValidationErrors(responseData?.errors);

      const apiError: ApiErrorWithStatus = new Error(
        responseData?.message ?? responseData?.error ?? responseData?.detail ?? responseData?.title ?? error.message,
      );
      apiError.status = error.response?.status;
      apiError.code = responseData?.code ?? (validationErrors ? 'VALIDATION_ERROR' : 'UNKNOWN_ERROR');
      apiError.message = responseData?.message ?? responseData?.error ?? responseData?.detail ?? responseData?.title ?? error.message;
      apiError.details = responseData?.details ?? (validationErrors ? { errors: validationErrors } : undefined);
      apiError.validationErrors = validationErrors;

      if (
        error.response?.status !== 401 ||
        !originalRequest ||
        isNonRefreshable(originalRequest.url) ||
        originalRequest._retry
      ) {
        return Promise.reject(apiError);
      }

      if (isRefreshing) {
        return new Promise<void>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(() => client(originalRequest))
          .catch((e) => Promise.reject(e));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // The HTTP-only refreshToken cookie is sent automatically.
        // No body is needed — omit Content-Type to avoid 415 from the server.
        await client.post(REFRESH_ENDPOINT, null, {
          headers: { 'Content-Type': undefined },
        });
        processQueue(null);
        return client(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError);
        window.dispatchEvent(new CustomEvent('auth:failure'));
        return Promise.reject(apiError);
      } finally {
        isRefreshing = false;
      }
    },
  );

  return client;
};

export const apiClient = createApiClient();

export default apiClient;
