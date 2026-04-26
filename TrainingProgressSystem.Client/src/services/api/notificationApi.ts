import { apiClient } from './client';

export interface ReminderResponse {
  message: string;
}

export const notificationApi = {
  getReminders: async (signal?: AbortSignal): Promise<ReminderResponse[]> => {
    const response = await apiClient.get<ReminderResponse[]>(
      '/notification-service/api/v1/reminders',
      { signal }
    );
    const data = response.data as unknown;
    if (Array.isArray(data)) return data as ReminderResponse[];
    const envelope = data as { value?: ReminderResponse[] };
    return envelope?.value ?? [];
  },
};
