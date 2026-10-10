import axios from 'axios';

interface ProblemDetails {
  message?: string;
  title?: string;
  errors?: Record<string, string[]>;
}

export function getErrorMessage(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (axios.isAxiosError(error)) {
    if (!error.response) return 'Cannot reach the server. Check your connection and that the API is running.';
    const data = error.response.data as ProblemDetails | string | undefined;
    if (typeof data === 'object' && data) {
      if (data.message) return data.message; // backend: { message }
      if (data.errors) return Object.values(data.errors).flat().join(' '); // ASP.NET validation
      if (data.title) return data.title;
    }
    if (error.response.status === 403) return 'You do not have permission to do this.';
    if (error.response.status === 404) return 'Not found.';
    return `${fallback} (HTTP ${error.response.status})`;
  }
  return error instanceof Error ? error.message : fallback;
}

export function getStatus(error: unknown): number | undefined {
  return axios.isAxiosError(error) ? error.response?.status : undefined;
}
