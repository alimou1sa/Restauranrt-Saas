import type { ReactNode } from 'react';
import { Spinner } from '../../components/Feedback';
import { getErrorMessage } from '../../utils/errors';

interface StatCardProps {
  label: string;
  value?: ReactNode;
  hint?: string;
  loading?: boolean;
  error?: unknown;
  /** Permission code the user lacks; renders a locked state. */
  lockedBy?: string;
  onRetry?: () => void;
}

export function StatCard({ label, value, hint, loading, error, lockedBy, onRetry }: StatCardProps) {
  return (
    <section className="card stat" aria-label={label}>
      <h2 className="stat-label">{label}</h2>
      {lockedBy ? (
        <p className="muted small">Not available for your role ({lockedBy}).</p>
      ) : loading ? (
        <div className="stat-body"><Spinner label={`Loading ${label}`} /></div>
      ) : error ? (
        <div role="alert">
          <p className="small error-text">{getErrorMessage(error)}</p>
          {onRetry && <button type="button" className="btn btn-link" onClick={onRetry}>Retry</button>}
        </div>
      ) : (
        <>
          <p className="stat-value">{value}</p>
          {hint && <p className="muted small">{hint}</p>}
        </>
      )}
    </section>
  );
}
