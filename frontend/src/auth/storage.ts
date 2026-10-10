// Token storage.
//  - PreAuth token (5 min, only valid for /api/auth endpoints): sessionStorage, never used as an access token.
//  - Session (access + refresh token): localStorage so a page reload keeps the user signed in.
// Trade-off: tokens in web storage are readable by any script on the page (XSS). The backend issues the
// refresh token in the JSON body, so an httpOnly cookie is not possible without a backend change.
import type { AuthTokenResponse, OrganizationOption } from '../types/api';

const SESSION_KEY = 'rs.session';
const PREAUTH_KEY = 'rs.preauth';

export interface StoredSession {
  accessToken: string;
  refreshToken: string;
  organizationId: number;
  organizationUserId: number;
  branchId: number | null;
}

export interface StoredPreAuth {
  token: string;
  organizations: OrganizationOption[];
}

function read<T>(storage: Storage, key: string): T | null {
  try {
    const raw = storage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : null;
  } catch {
    return null;
  }
}

function write(storage: Storage, key: string, value: unknown): void {
  try {
    storage.setItem(key, JSON.stringify(value));
  } catch {
    /* storage unavailable (private mode / quota): session just won't persist */
  }
}

export const sessionStore = {
  get: () => read<StoredSession>(localStorage, SESSION_KEY),
  set: (t: AuthTokenResponse) =>
    write(localStorage, SESSION_KEY, {
      accessToken: t.token,
      refreshToken: t.refreshToken,
      organizationId: t.organizationId,
      organizationUserId: t.organizationUserId,
      branchId: t.branchId,
    } satisfies StoredSession),
  clear: () => localStorage.removeItem(SESSION_KEY),
  getAccessToken: () => sessionStore.get()?.accessToken ?? null,
  getRefreshToken: () => sessionStore.get()?.refreshToken ?? null,
};

export const preAuthStore = {
  get: () => read<StoredPreAuth>(sessionStorage, PREAUTH_KEY),
  set: (v: StoredPreAuth) => write(sessionStorage, PREAUTH_KEY, v),
  clear: () => sessionStorage.removeItem(PREAUTH_KEY),
};
