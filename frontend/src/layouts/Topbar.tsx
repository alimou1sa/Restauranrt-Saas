import { useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { Icon } from '../components/Icon';
import { useToast } from '../components/Toast';
import { fullName, initials } from '../utils/format';

export function Topbar({ onMenu }: { onMenu: () => void }) {
  const { me, logout } = useAuth();
  const toast = useToast();
  const [busy, setBusy] = useState(false);
  if (!me) return null;

  const name = fullName(me.firstName, me.lastName);

  async function handleLogout() {
    setBusy(true);
    await logout();
    toast.info('You have been signed out.');
  }

  return (
    <header className="topbar">
      <button type="button" className="icon-btn menu-btn" onClick={onMenu} aria-label="Open menu">
        <Icon name="hamburger" />
      </button>

      <div className="context" aria-label="Current organization and branch">
        <Icon name="building" />
        <div className="context-text">
          <strong>{me.organizationName}</strong>
          <span className="muted">{me.branchName ?? 'All branches'}</span>
        </div>
      </div>

      <div className="topbar-right">
        <div className="user">
          <span className="avatar" aria-hidden="true">{initials(me.firstName, me.lastName)}</span>
          <div className="user-text">
            <strong>{name}</strong>
            <span className="muted">{me.email}</span>
          </div>
        </div>
        <button type="button" className="btn btn-secondary btn-sm" onClick={handleLogout} disabled={busy}>
          <Icon name="logout" size={16} />
          <span className="hide-sm">{busy ? 'Signing out…' : 'Log out'}</span>
        </button>
      </div>
    </header>
  );
}
