import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { authApi } from '../api/auth';
import type { MeResponse, OrganizationOption } from '../types/api';
import { getErrorMessage, getStatus } from '../utils/errors';
import { isTokenExpired } from '../utils/jwt';
import { authEvents } from './events';
import { refreshSession } from './refresh';
import { preAuthStore, sessionStore } from './storage';

// loading: restoring a stored session | preauth: logged in, must pick an organization
// error: could not restore the session for a non-auth reason (network down, /auth/me missing, ...)
export type AuthStatus = 'loading' | 'unauthenticated' | 'preauth' | 'authenticated' | 'error';

interface AuthContextValue {
  status: AuthStatus;
  me: MeResponse | null;
  organizations: OrganizationOption[];
  notice: string | null;
  bootError: string | null;
  login: (email: string, password: string) => Promise<void>;
  selectOrganization: (organizationId: number) => Promise<void>;
  cancelOrganizationSelection: () => void;
  logout: () => Promise<void>;
  retryBootstrap: () => void;
  /** UI hint only. The backend re-checks every request; never treat this as a security boundary. */
  hasPermission: (code: string) => boolean;
  hasAnyPermission: (codes: readonly string[]) => boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const MISSING_ME_MESSAGE =
  'The API has no GET /api/auth/me endpoint. Apply backend-patch/BACKEND_PATCH.md to the backend and restart it.';

async function fetchMe(): Promise<MeResponse> {
  try {
    return await authApi.me();
  } catch (e) {
    if (getStatus(e) === 404) throw new Error(MISSING_ME_MESSAGE);
    throw e;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>('loading');
  const [me, setMe] = useState<MeResponse | null>(null);
  const [organizations, setOrganizations] = useState<OrganizationOption[]>([]);
  const [notice, setNotice] = useState<string | null>(null);
  const [bootError, setBootError] = useState<string | null>(null);
  const [bootAttempt, setBootAttempt] = useState(0);

  // Restore state on page load.
  useEffect(() => {
    let cancelled = false;
    (async () => {
      setStatus('loading');
      setBootError(null);

      const session = sessionStore.get();
      if (session) {
        try {
          if (isTokenExpired(session.accessToken)) await refreshSession();
          const profile = await fetchMe();
          if (cancelled) return;
          setMe(profile);
          setStatus('authenticated');
          return;
        } catch (e) {
          if (cancelled) return;
          if (!sessionStore.get() || getStatus(e) === 401) {
            sessionStore.clear(); // definitively signed out -> fall through to login
          } else {
            setBootError(getErrorMessage(e));
            setStatus('error');
            return;
          }
        }
      }

      const pre = preAuthStore.get();
      if (pre && !isTokenExpired(pre.token, 0)) {
        setOrganizations(pre.organizations);
        setStatus('preauth');
        return;
      }
      preAuthStore.clear();
      setStatus('unauthenticated');
    })();
    return () => {
      cancelled = true;
    };
  }, [bootAttempt]);

  // The HTTP layer reports ended sessions (refresh rejected, PreAuth token expired).
  useEffect(
    () =>
      authEvents.subscribe((reason) => {
        setMe(null);
        setOrganizations([]);
        setNotice(
          reason === 'expired'
            ? 'Your session has expired. Please sign in again.'
            : 'Organization selection timed out. Please sign in again.',
        );
        setStatus('unauthenticated');
      }),
    [],
  );

  const login = useCallback(async (email: string, password: string) => {
    const res = await authApi.login({ email, password });
    // The login response is a PreAuth token. It only unlocks /api/auth endpoints, never the app.
    if (res.tokenType !== 'org_selection') throw new Error('Unexpected login response from the server.');
    sessionStore.clear();
    preAuthStore.set({ token: res.token, organizations: res.organizations });
    setOrganizations(res.organizations);
    setNotice(null);
    setStatus('preauth');
  }, []);

  const selectOrganization = useCallback(async (organizationId: number) => {
    const tokens = await authApi.selectOrganization(organizationId);
    sessionStore.set(tokens);
    let profile: MeResponse;
    try {
      profile = await fetchMe();
    } catch (e) {
      sessionStore.clear();
      throw e;
    }
    preAuthStore.clear();
    setOrganizations([]);
    setMe(profile);
    setStatus('authenticated');
  }, []);

  const cancelOrganizationSelection = useCallback(() => {
    preAuthStore.clear();
    setOrganizations([]);
    setStatus('unauthenticated');
  }, []);

  const logout = useCallback(async () => {
    if (sessionStore.get()) {
      try {
        // Refresh first if needed so we revoke the *latest* refresh token, not an already-rotated one.
        const token = sessionStore.getAccessToken();
        if (token && isTokenExpired(token)) await refreshSession();
        const refreshToken = sessionStore.getRefreshToken();
        if (refreshToken) await authApi.logout(refreshToken);
      } catch {
        /* best effort: always sign out locally */
      }
    }
    sessionStore.clear();
    preAuthStore.clear();
    setMe(null);
    setOrganizations([]);
    setNotice(null);
    setStatus('unauthenticated');
  }, []);

  const retryBootstrap = useCallback(() => setBootAttempt((n) => n + 1), []);

  const permissionSet = useMemo(() => new Set(me?.permissions ?? []), [me]);
  const hasPermission = useCallback((code: string) => permissionSet.has(code), [permissionSet]);
  const hasAnyPermission = useCallback(
    (codes: readonly string[]) => codes.some((c) => permissionSet.has(c)),
    [permissionSet],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      status, me, organizations, notice, bootError,
      login, selectOrganization, cancelOrganizationSelection, logout, retryBootstrap,
      hasPermission, hasAnyPermission,
    }),
    [status, me, organizations, notice, bootError, login, selectOrganization, cancelOrganizationSelection,
      logout, retryBootstrap, hasPermission, hasAnyPermission],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>.');
  return ctx;
}
