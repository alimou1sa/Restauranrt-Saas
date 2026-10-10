// The ONE axios instance. Components never configure axios themselves.
import axios, { AxiosError } from 'axios';
import { authEvents } from '../auth/events';
import { refreshSession } from '../auth/refresh';
import { preAuthStore, sessionStore } from '../auth/storage';
import { API_BASE_URL } from '../config/env';

declare module 'axios' {
  export interface AxiosRequestConfig {
    /** access (default): Bearer access token. preauth: Bearer PreAuth token. none: no Authorization header. */
    authMode?: 'access' | 'preauth' | 'none';
    _retried?: boolean;
  }
}

export const http = axios.create({
  baseURL: API_BASE_URL,
  timeout: 20_000,
  headers: { 'Content-Type': 'application/json' },
});

http.interceptors.request.use((config) => {
  const mode = config.authMode ?? 'access';
  const token =
    mode === 'access' ? sessionStore.getAccessToken() : mode === 'preauth' ? preAuthStore.get()?.token : null;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

http.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config;
    if (!original || error.response?.status !== 401) throw error;

    const mode = original.authMode ?? 'access';

    // PreAuth token expired (5 min lifetime): nothing to refresh, user must log in again.
    if (mode === 'preauth') {
      preAuthStore.clear();
      authEvents.emit('preauth-expired');
      throw error;
    }

    // Login itself (mode none) or an already-retried request: a 401 is a real answer.
    if (mode === 'none' || original._retried) throw error;

    if (!sessionStore.getRefreshToken()) {
      authEvents.emit('expired');
      throw error;
    }

    try {
      await refreshSession(); // shared promise: concurrent 401s wait for the same refresh
    } catch {
      throw error; // refreshSession already cleared the session when the refresh was rejected
    }

    original._retried = true;
    return http(original); // request interceptor attaches the new access token
  },
);
