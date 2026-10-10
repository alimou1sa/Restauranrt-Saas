import type { ReactNode } from 'react';
import { Icon, type IconName } from '../../components/Icon';
import { Spinner } from '../../components/Feedback';
import { getErrorMessage } from '../../utils/errors';

interface StatCardProps {
  label: string;
  value?: ReactNode;
  hint?: string;
  icon: IconName;
  tone?: 'orange' | 'blue' | 'green' | 'violet';
  loading?: boolean;
  error?: unknown;
  /** Permission code the user lacks; renders a locked state. */
  lockedBy?: string;
  onRetry?: () => void;
}

export function StatCard({
  label, value, hint, icon, tone = 'orange', loading, error, lockedBy, onRetry,
}: StatCardProps) {
  return (
    <section className="card stat" aria-label={label}>
      <div className="stat-top">
        <h2 className="stat-label">{label}</h2>
        <span className={`stat-icon stat-icon-${tone}`} aria-hidden="true"><Icon name={icon} size={19} /></span>
      </div>
      {lockedBy ? (
        <p className="muted small stat-message">Not available for your role ({lockedBy}).</p>
      ) : loading ? (
        <div className="stat-body"><Spinner label={`Loading ${label}`} /></div>
      ) : error ? (
        <div role="alert" className="stat-error">
          <p className="small error-text">{getErrorMessage(error)}</p>
          {onRetry && <button type="button" className="btn btn-link" onClick={onRetry}>Try again</button>}
        </div>
      ) : (
        <>
          <p className="stat-value">{value ?? '—'}</p>
          {hint && <p className="muted small stat-hint">{hint}</p>}
        </>
      )}
    </section>
  );
}
