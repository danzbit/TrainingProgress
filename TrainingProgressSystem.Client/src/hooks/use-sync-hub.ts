import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAppSelector } from '@/store/hooks';

const HUB_URL = (import.meta.env.VITE_API_BASE_URL || '') + '/hubs/sync';

/**
 * Connects to the BFF SignalR hub while the user is authenticated.
 * Dispatches window CustomEvents so any page can react without prop-drilling:
 *   - 'sync:workout_created'  → detail: { workoutId: string }
 *   - 'sync:goal_saved'       → detail: { goalId: string }
 */
export function useSyncHub() {
  const isAuthenticated = useAppSelector((s) => s.auth.isAuthenticated);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        // The HttpOnly accessToken cookie is sent automatically by the browser.
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('WorkoutCreated', (data: { workoutId: string }) => {
      window.dispatchEvent(new CustomEvent('sync:workout_created', { detail: data }));
    });

    connection.on('GoalSaved', (data: { goalId: string }) => {
      window.dispatchEvent(new CustomEvent('sync:goal_saved', { detail: data }));
    });

    connection
      .start()
      .catch((err) => console.warn('[SyncHub] connection failed:', err));

    connectionRef.current = connection;

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [isAuthenticated]);
}
