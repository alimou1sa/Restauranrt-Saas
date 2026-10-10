import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { platformApi } from '../api/platform';
import type { OrganizationResponse, UserResponse, PlanResponse } from '../types/api';
import { ErrorState, Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

export function PlatformAdminOverviewPage() {
  const [orgs, setOrgs] = useState<OrganizationResponse[] | null>(null);
  const [users, setUsers] = useState<UserResponse[] | null>(null);
  const [plans, setPlans] = useState<PlanResponse[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => { let live = true; Promise.all([platformApi.organizations(), platformApi.users(), platformApi.plans()])
    .then(([o,u,p]) => { if (live) { setOrgs(o); setUsers(u); setPlans(p); } })
    .catch(e => { if (live) setError(getErrorMessage(e)); });
    return () => { live = false; };
  }, []);
  const loading = !error && (orgs === null || users === null || plans === null);
  return <>
    <div className="admin-page-heading"><div><span className="platform-eyebrow">OVERVIEW</span><h1>Good day, administrator.</h1><p>Monitor the platform and manage your restaurant workspaces from one place.</p></div></div>
    {error && <ErrorState error={error} onRetry={() => window.location.reload()} />}
    {loading && <div className="admin-loading"><Spinner label="Loading platform data" /> Loading platform data…</div>}
    {!loading && !error && <>
      <section className="admin-stat-grid">
        <article className="admin-stat"><span>Organizations</span><strong>{orgs?.length ?? 0}</strong><small>{orgs?.filter(x=>x.isActive).length ?? 0} active workspaces</small></article>
        <article className="admin-stat"><span>User accounts</span><strong>{users?.length ?? 0}</strong><small>{users?.filter(x=>x.isActive).length ?? 0} active accounts</small></article>
        <article className="admin-stat"><span>Plans</span><strong>{plans?.length ?? 0}</strong><small>{plans?.filter(x=>x.isActive).length ?? 0} available plans</small></article>
      </section>
      <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Recent organizations</h2><p>Latest workspaces registered on the platform</p></div><Link to="/platform-admin/organizations">View all</Link></div>
        <div className="admin-table-wrap"><table><thead><tr><th>Organization</th><th>Slug</th><th>Status</th><th>Created</th></tr></thead><tbody>
          {[...(orgs ?? [])].sort((a,b)=>Date.parse(b.createdAtUtc)-Date.parse(a.createdAtUtc)).slice(0,5).map(o=><tr key={o.organizationId}><td><strong>{o.name}</strong><small>{o.email || 'No contact email'}</small></td><td>{o.slug}</td><td><span className={`admin-status ${o.isActive?'is-active':'is-inactive'}`}>{o.isActive?'Active':'Inactive'}</span></td><td>{new Date(o.createdAtUtc).toLocaleDateString()}</td></tr>)}
          {!orgs?.length && <tr><td colSpan={4}>No organizations found.</td></tr>}
        </tbody></table></div>
      </section>
    </>}
  </>;
}
