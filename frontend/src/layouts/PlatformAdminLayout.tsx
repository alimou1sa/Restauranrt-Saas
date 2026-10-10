import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function PlatformAdminLayout() {
  const { me, logout } = useAuth();
  const navigate = useNavigate();
  async function signOut() { await logout(); navigate('/login', { replace: true }); }
  return <div className="platform-shell">
    <aside className="platform-sidebar">
      <div className="platform-brand"><span className="brand-mark">R</span><span>RestaurantSaaS</span></div>
      <p className="platform-eyebrow">PLATFORM</p>
      <nav className="platform-nav">
        <NavLink end to="/platform-admin">Overview</NavLink>
        <NavLink to="/platform-admin/organizations">Organizations</NavLink>
        <NavLink to="/platform-admin/users">Users</NavLink>
        <NavLink to="/platform-admin/plans">Plans & pricing</NavLink>
      </nav>
      <div className="platform-sidebar-bottom"><span>Platform administrator</span><strong>{me?.email}</strong>
        <button className="btn btn-secondary btn-block" onClick={signOut}>Sign out</button></div>
    </aside>
    <main className="platform-main"><header className="platform-topbar"><div><span className="platform-eyebrow">CONTROL CENTER</span><strong>Platform administration</strong></div><span className="admin-avatar">A</span></header>
      <div className="platform-content"><Outlet /></div>
    </main>
  </div>;
}
