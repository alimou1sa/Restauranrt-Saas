import type { ReactNode } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { ErrorState, FullPageLoader } from '../components/Feedback';

function BootGate({ children }: { children: ReactNode }) {
  const { status, bootError, retryBootstrap } = useAuth();
  if (status === 'loading') return <FullPageLoader label="Restoring your session…" />;
  if (status === 'error') {
    return (
      <div className="center-screen">
        <div className="card narrow">
          <h1 className="h2">Could not restore your session</h1>
          <ErrorState error={new Error(bootError ?? 'Unknown error')} onRetry={retryBootstrap} />
        </div>
      </div>
    );
  }
  return <>{children}</>;
}

/** Only for fully authenticated users (final access token + loaded profile). */
export function RequireAuth() {
  const { status } = useAuth();
  const location = useLocation();
  return (
    <BootGate>
      {status === 'authenticated' ? (
        <Outlet />
      ) : status === 'preauth' ? (
        <Navigate to="/select-organization" replace state={{ from: location }} />
      ) : (
        <Navigate to="/login" replace state={{ from: location }} />
      )}
    </BootGate>
  );
}

/** Login page: signed-in users are sent on to wherever they should be. */
export function PublicOnly({ children }: { children: ReactNode }) {
  const { status } = useAuth();
  const location = useLocation();
  return (
    <BootGate>
      {status === 'authenticated' ? (
        <Navigate to="/dashboard" replace />
      ) : status === 'preauth' ? (
        <Navigate to="/select-organization" replace state={location.state} />
      ) : (
        <>{children}</>
      )}
    </BootGate>
  );
}

/** Organization selection requires the PreAuth state. */
export function RequirePreAuth({ children }: { children: ReactNode }) {
  const { status } = useAuth();
  return (
    <BootGate>
      {status === 'preauth' ? <>{children}</> : <Navigate to={status === 'authenticated' ? '/dashboard' : '/login'} replace />}
    </BootGate>
  );
}

/** Route-level permission check for feature pages added later: <RequirePermission code="order.read">. */
export function RequirePermission({ code, children }: { code: string; children: ReactNode }) {
  const { hasPermission } = useAuth();
  if (hasPermission(code)) return <>{children}</>;
  return (
    <div className="card narrow">
      <h1 className="h2">No access</h1>
      <p className="muted">Your role does not include the “{code}” permission. Ask an administrator if you need it.</p>
    </div>
  );
}
