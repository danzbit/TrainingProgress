import apiClient from './client';

const BFF_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';
const AI_CHAT_ENDPOINT = '/ai-chat-service/api/v1/chat';

export interface ChatMessageDto {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
}

export const aiChatApi = {
  getHistory: async (): Promise<ChatMessageDto[]> => {
    const res = await apiClient.get<ChatMessageDto[]>(`${AI_CHAT_ENDPOINT}/history`);
    return res.data;
  },

  /**
   * Opens an SSE stream to /chat/stream. Calls onChunk for each text delta,
   * onDone when the stream closes, and onError on failure.
   */
  streamMessage: (
    message: string,
    onChunk: (chunk: string) => void,
    onDone: () => void,
    onError: (err: string) => void,
    signal?: AbortSignal
  ): void => {
    // We use fetch directly to handle SSE streaming
    fetch(`${BFF_BASE_URL}${AI_CHAT_ENDPOINT}/stream`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ message }),
      signal,
    })
      .then(async (response) => {
        if (!response.ok) {
          onError(`HTTP error ${response.status}`);
          return;
        }

        const reader = response.body?.getReader();
        if (!reader) { onError('No response body'); return; }

        const decoder = new TextDecoder();
        let buffer = '';

        while (true) {
          const { done, value } = await reader.read();
          if (done) break;

          buffer += decoder.decode(value, { stream: true });
          const lines = buffer.split('\n');
          buffer = lines.pop() ?? '';

          for (const line of lines) {
            if (!line.startsWith('data: ')) continue;
            const data = line.slice('data: '.length).trim();
            if (data === '[DONE]') { onDone(); return; }
            try {
              const chunk = JSON.parse(data) as string;
              onChunk(chunk);
            } catch {
              // ignore parse errors
            }
          }
        }
        onDone();
      })
      .catch((err: Error) => {
        if (err.name !== 'AbortError') onError(err.message);
      });
  },

  clearHistory: async (): Promise<void> => {
    await apiClient.delete(`${AI_CHAT_ENDPOINT}/history`);
  },
};
