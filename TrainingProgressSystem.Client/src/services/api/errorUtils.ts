import type { ApiErrorWithStatus } from './types';

const getValidationMessages = (validationErrors?: Record<string, string[]>): string[] => {
  if (!validationErrors) {
    return [];
  }

  return [...new Set(Object.values(validationErrors).flat().map((message) => message.trim()).filter(Boolean))];
};

export const getApiErrorDescription = (error: unknown, fallbackMessage: string): string => {
  if (!(error instanceof Error)) {
    return fallbackMessage;
  }

  const apiError = error as ApiErrorWithStatus;
  const validationMessages = getValidationMessages(apiError.validationErrors);

  if (validationMessages.length > 0) {
    return validationMessages.join('\n');
  }

  if (apiError.details && 'errors' in apiError.details) {
    const validationErrors = apiError.details.errors;
    if (validationErrors && typeof validationErrors === 'object' && !Array.isArray(validationErrors)) {
      const messages = getValidationMessages(validationErrors as Record<string, string[]>);
      if (messages.length > 0) {
        return messages.join('\n');
      }
    }
  }

  return apiError.message?.trim() || fallbackMessage;
};