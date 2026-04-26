/**
 * API Response Types and Interfaces
 */

export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
    details?: Record<string, unknown>;
  };
}

export interface ApiErrorResponse {
  code: string;
  message: string;
  details?: Record<string, unknown>;
}

export type ApiValidationErrors = Record<string, string[]>;

export interface ApiErrorWithStatus extends Error {
  status?: number;
  code?: string;
  details?: Record<string, unknown>;
  validationErrors?: ApiValidationErrors;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ApiRequestConfig {
  headers?: Record<string, string>;
  params?: Record<string, unknown>;
  timeout?: number;
}

// ── Auth types ──────────────────────────────────────────────────────────────

export interface LoginRequest {
  userNameOrEmail: string;
  password: string;
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  id: string;
  token: string;
  refreshToken: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
}

export interface CurrentUserResponse {
  id: string;
  userName: string;
  email: string;
}
