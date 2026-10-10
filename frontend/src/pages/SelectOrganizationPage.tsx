import { useEffect, useRef, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { authApi } from '../api/auth';
import { useAuth } from '../auth/AuthContext';
import { EmptyState, ErrorState, Spinner } from '../components/Feedback';
import { Icon } from '../components/Icon';
import { useToast } from '../components/Toast';
import { useAsync } from '../hooks/useAsync';
import { getErrorMessage } from '../utils/errors';

export function SelectOrganizationPage() {
  const { selectOrganization, cancelOrganizationSelection, organizations: stored } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();
  const [pendingId, setPendingId] = useState<number | null>(null);
  const [selectError, setSelectError] = useState<string | null>(null);
  const autoTried = useRef(false);

  // GET /api/auth with the PreAuth token (also validates that the token is still alive).
  const { data, error, loading, reload } = useAsync(() => authApi.myOrganizations(), []);
  const options = data ?? (error ? stored : []);

  const from = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname;

  async function choose(organizationId: number) {
    setPendingId(organizationId);
    setSelectError(null);
    try {
      await selectOrganization(organizationId);
      navigate(from && from !== '/login' ? from : '/dashboard', { replace: true });
    } catch (e) {
      const message = getErrorMessage(e, 'Could not open this organization.');
      setSelectError(message);
      toast.error(message);
      setPendingId(null);
    }
  }

  // A user with exactly one membership has nothing to choose: continue automatically (once).
  useEffect(() => {
    if (!autoTried.current && data && data.length === 1) {
      autoTried.current = true;
      void choose(data[0].organizationId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data]);

  return (
    <div className="center-screen">
      <div className="card narrow">
        <h1 className="h2">Choose a workspace</h1>
        <p className="muted">Select the organization you want to work in.</p>

        {selectError && <div className="banner banner-error" role="alert">{selectError}</div>}

        {loading && <div className="state"><Spinner label="Loading organizations" /></div>}
        {!loading && error && !options.length && <ErrorState error={error} onRetry={reload} />}
        {!loading && !error && options.length === 0 && (
          <EmptyState title="No organizations" description="Your account is not assigned to any active organization." />
        )}

        <ul className="org-list">
          {options.map((o) => (
            <li key={o.organizationId}>
              <button type="button" className="org-item" onClick={() => choose(o.organizationId)} disabled={pendingId !== null}>
                <Icon name="building" size={20} />
                <span className="org-text">
                  <strong>{o.organizationName}</strong>
                  <span className="muted">{o.branchName ?? 'All branches'}</span>
                </span>
                {pendingId === o.organizationId && <Spinner label="Opening" />}
              </button>
            </li>
          ))}
        </ul>

        <button type="button" className="btn btn-link" onClick={cancelOrganizationSelection}>
          Sign in with a different account
        </button>
      </div>
    </div>
  );
}
