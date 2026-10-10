// The ONE axios instance. Components never configure axios themselves.
import axios, { AxiosError } from 'axios';
import { authEvents } from '../auth/events';
import { refreshSession } from '../auth/refresh';
import { platformAdminStore, preAuthStore, sessionStore } from '../auth/storage';
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
  const token = mode === 'access'
    ? (platformAdminStore.getAccessToken() ?? sessionStore.getAccessToken())
    : mode === 'preauth' ? preAuthStore.get()?.token : null;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

http.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config;
    if (!original || error.response?.status !== 401) throw error;
    const mode = original.authMode ?? 'access';
    if (mode === 'preauth') {
      preAuthStore.clear(); authEvents.emit('preauth-expired'); throw error;
    }
    if (mode === 'none' || original._retried) throw error;
    // Platform-admin tokens do not have a refresh token in the current API.
    if (platformAdminStore.get()) {
      platformAdminStore.clear(); authEvents.emit('expired'); throw error;
    }
    if (!sessionStore.getRefreshToken()) {
      authEvents.emit('expired'); throw error;
    }
    try { await refreshSession(); }
    catch { throw error; }
    original._retried = true;
    return http(original);
  },
);
