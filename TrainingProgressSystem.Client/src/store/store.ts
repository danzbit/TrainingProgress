import { configureStore } from '@reduxjs/toolkit';
import workoutReducer, { saveWorkoutState } from './workoutSlice';
import authReducer from './authSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    workout: workoutReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: false,
    }),
});

store.subscribe(() => {
  saveWorkoutState(store.getState().workout);
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
