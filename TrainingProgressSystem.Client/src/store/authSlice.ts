import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { CurrentUserResponse } from '@/services/api/types';

export interface AuthState {
  user: CurrentUserResponse | null;
  isAuthenticated: boolean;
  /** True while the initial /me bootstrap check is in-flight */
  isLoading: boolean;
}

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: true, // assume unknown until bootstrap completes
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setUser(state, action: PayloadAction<CurrentUserResponse>) {
      state.user = action.payload;
      state.isAuthenticated = true;
      state.isLoading = false;
    },
    clearUser(state) {
      state.user = null;
      state.isAuthenticated = false;
      state.isLoading = false;
    },
    setAuthLoading(state, action: PayloadAction<boolean>) {
      state.isLoading = action.payload;
    },
  },
});

export const { setUser, clearUser, setAuthLoading } = authSlice.actions;
export default authSlice.reducer;
