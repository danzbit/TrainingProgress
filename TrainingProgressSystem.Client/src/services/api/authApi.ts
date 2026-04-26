import apiClient from './client';
import type { ApiErrorWithStatus } from './types';
import type { LoginRequest, RegisterRequest, LoginResponse, CurrentUserResponse } from './types';

const AUTH_ENDPOINT = '/auth-service/api/v1/auth';

/**
 * Auth API endpoints.
 * Auth routes bypass the generic ApiResponse<T> envelope — the
 * server returns raw objects (or 200 No Content) directly via ToActionResult().
 * Cookies (accessToken / refreshToken) are set as HTTP-only by the server.
 */
export const authApi = {
  /**
   * Sign in with username/email + password.
   * The server sets HTTP-only access and refresh token cookies on success.
   */
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    try {
      const response = await apiClient.post<LoginResponse>(
        `${AUTH_ENDPOINT}/sign-in`,
        credentials,
      );
      return response.data;
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },

  /**
   * Create a new account.
   * Returns 200 OK with no body on success.
   */
  register: async (payload: RegisterRequest): Promise<void> => {
    try {
      await apiClient.post(`${AUTH_ENDPOINT}/sign-up`, payload);
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },

  /**
   * Fetch the currently authenticated user (requires valid accessToken cookie).
   */
  getCurrentUser: async (): Promise<CurrentUserResponse> => {
    try {
      const response = await apiClient.get<CurrentUserResponse>(`${AUTH_ENDPOINT}/me`);
      return response.data;
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },

  /**
   * Use the HTTP-only refresh token cookie to obtain a new access token cookie.
   * Called automatically by the axios interceptor – rarely needed directly.
   */
  refreshToken: async (): Promise<void> => {
    try {
      await apiClient.post(`${AUTH_ENDPOINT}/refresh-token`);
    } catch (error) {
      throw error as ApiErrorWithStatus;
    }
  },

  /**
   * Invalidate server-side session and clear HTTP-only cookies.
   */
  logout: async (): Promise<void> => {
    try {
      await apiClient.post(`${AUTH_ENDPOINT}/sign-out`);
    } catch {
      // Ignore errors – local state is cleared regardless
    }
  },
};
