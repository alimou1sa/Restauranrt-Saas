// Single-flight refresh. The backend ROTATES refresh tokens and treats reuse of an old one as theft
// (it revokes every session of the membership), so two concurrent refresh calls must never happen.
import axios from 'axios';
import { API_BASE_URL } from '../config/env';
import type { AuthTokenResponse } from '../types/api';
import { authEvents } from './events';
import { sessionStore } from './storage';

// Bare instance: no interceptors, so a failing refresh can't trigger another refresh.
const bare = axios.create({ baseURL: API_BASE_URL, timeout: 20_000 });

let inflight: Promise<AuthTokenResponse> | null = null;

export function refreshSession(): Promise<AuthTokenResponse> {
  if (inflight) return inflight;

  const refreshToken = sessionStore.getRefreshToken();
  if (!refreshToken) return Promise.reject(new Error('No refresh token available.'));

  inflight = bare
    .post<AuthTokenResponse>('/auth/refresh', { refreshToken })
    .then((res) => {
      sessionStore.set(res.data);
      return res.data;
    })
    .catch((err: unknown) => {
      // Only a definitive rejection ends the session; a network blip keeps it so the user can retry.
      if (axios.isAxiosError(err) && err.response && (err.response.status === 401 || err.response.status === 400)) {
        sessionStore.clear();
        authEvents.emit('expired');
      }
      throw err;
    })
    .finally(() => {
      inflight = null;
    });

  return inflight;
}
