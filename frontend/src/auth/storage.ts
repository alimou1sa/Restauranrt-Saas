// Web storage is used because the current API returns tokens in JSON. Keep the short-lived
// organization-selection token separate from full organization and platform-admin sessions.
import type { AuthTokenResponse, OrganizationOption } from '../types/api';

const SESSION_KEY = 'rs.session';
const PREAUTH_KEY = 'rs.preauth';
const PLATFORM_ADMIN_KEY = 'rs.platform-admin';

export interface StoredSession {
  accessToken: string; refreshToken: string; organizationId: number; organizationUserId: number; branchId: number | null;
}
export interface StoredPreAuth { token: string; organizations: OrganizationOption[] }
export interface StoredPlatformAdmin { accessToken: string }

function read<T>(storage: Storage, key: string): T | null {
  try { const raw = storage.getItem(key); return raw ? JSON.parse(raw) as T : null; } catch { return null; }
}
function write(storage: Storage, key: string, value: unknown): void {
  try { storage.setItem(key, JSON.stringify(value)); } catch { /* persistence is best effort */ }
}

export const platformAdminStore = {
  get: () => read<StoredPlatformAdmin>(localStorage, PLATFORM_ADMIN_KEY),
  set: (token: string) => write(localStorage, PLATFORM_ADMIN_KEY, { accessToken: token } satisfies StoredPlatformAdmin),
  clear: () => localStorage.removeItem(PLATFORM_ADMIN_KEY),
  getAccessToken: () => platformAdminStore.get()?.accessToken ?? null,
};
export const sessionStore = {
  get: () => read<StoredSession>(localStorage, SESSION_KEY),
  set: (t: AuthTokenResponse) => write(localStorage, SESSION_KEY, {
    accessToken: t.token, refreshToken: t.refreshToken, organizationId: t.organizationId,
    organizationUserId: t.organizationUserId, branchId: t.branchId,
  } satisfies StoredSession),
  clear: () => localStorage.removeItem(SESSION_KEY),
  getAccessToken: () => sessionStore.get()?.accessToken ?? platformAdminStore.getAccessToken(),
  getRefreshToken: () => sessionStore.get()?.refreshToken ?? null,
};
export const preAuthStore = {
  get: () => read<StoredPreAuth>(sessionStorage, PREAUTH_KEY),
  set: (v: StoredPreAuth) => write(sessionStorage, PREAUTH_KEY, v),
  clear: () => sessionStorage.removeItem(PREAUTH_KEY),
};
