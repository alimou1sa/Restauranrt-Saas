import type { ReactNode } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { ErrorState, FullPageLoader } from '../components/Feedback';

function BootGate({ children }: { children: ReactNode }) {
  const { status, bootError, retryBootstrap } = useAuth();
  if (status === 'loading') return <FullPageLoader label="Restoring your session…" />;
  if (status === 'error') return <div className="center-screen"><div className="card narrow">
    <h1 className="h2">Could not restore your session</h1>
    <ErrorState error={new Error(bootError ?? 'Unknown error')} onRetry={retryBootstrap} />
  </div></div>;
  return <>{children}</>;
}

export function RequireAuth() {
  const { status, me } = useAuth();
  const location = useLocation();
  return <BootGate>{status === 'authenticated'
    ? me?.isPlatformAdmin ? <Navigate to="/platform-admin" replace /> : <Outlet />
    : status === 'preauth' ? <Navigate to="/select-organization" replace state={{ from: location }} />
    : <Navigate to="/login" replace state={{ from: location }} />}</BootGate>;
}

export function PublicOnly({ children }: { children: ReactNode }) {
  const { status, me } = useAuth();
  const location = useLocation();
  return <BootGate>{status === 'authenticated'
    ? <Navigate to={me?.isPlatformAdmin ? '/platform-admin' : '/dashboard'} replace />
    : status === 'preauth' ? <Navigate to="/select-organization" replace state={location.state} />
    : <>{children}</>}</BootGate>;
}

export function RequirePreAuth({ children }: { children: ReactNode }) {
  const { status } = useAuth();
  return <BootGate>{status === 'preauth' ? <>{children}</>
    : <Navigate to={status === 'authenticated' ? '/dashboard' : '/login'} replace />}</BootGate>;
}

export function RequirePlatformAdmin() {
  const { status, me } = useAuth();
  return <BootGate>{status !== 'authenticated' ? <Navigate to="/login" replace />
    : me?.isPlatformAdmin ? <Outlet /> : <Navigate to="/dashboard" replace />}</BootGate>;
}

export function RequirePermission({ code, children }: { code: string; children: ReactNode }) {
  const { hasPermission } = useAuth();
  if (hasPermission(code)) return <>{children}</>;
  return <div className="card narrow"><h1 className="h2">No access</h1>
    <p className="muted">Your role does not include the “{code}” permission. Ask an administrator if you need it.</p></div>;
}
