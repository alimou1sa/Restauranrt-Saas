import { useEffect, useState } from 'react';
import { platformApi } from '../api/platform';
import type { OrganizationResponse } from '../types/api';
import { ErrorState, Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

export function PlatformOrganizationsPage() {
  const [items,setItems] = useState<OrganizationResponse[]|null>(null);
  const [error,setError] = useState<string|null>(null);
  const [busyId,setBusyId] = useState<number|null>(null);
  const [query,setQuery] = useState('');
  async function load(){setError(null);try{setItems(await platformApi.organizations())}catch(e){setError(getErrorMessage(e))}}
  useEffect(()=>{void load()},[]);
  async function toggle(item:OrganizationResponse){setBusyId(item.organizationId);try{const updated=await platformApi.setOrganizationActive(item.organizationId,{isActive:!item.isActive});setItems(old=>old?.map(x=>x.organizationId===updated.organizationId?updated:x)??[])}catch(e){setError(getErrorMessage(e))}finally{setBusyId(null)}}
  const filtered=(items??[]).filter(x=>[x.name,x.slug,x.email??''].some(v=>v.toLowerCase().includes(query.toLowerCase())));
  return <>
    <div className="admin-page-heading"><div><span className="platform-eyebrow">TENANT MANAGEMENT</span><h1>Organizations</h1><p>Review customer workspaces and control whether they can access the platform.</p></div><span className="admin-count">{items?.length??'—'} total</span></div>
    <section className="admin-panel"><div className="admin-toolbar"><input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Search organizations…" aria-label="Search organizations" /><button className="btn btn-secondary" onClick={()=>void load()}>Refresh</button></div>
      {error&&<ErrorState error={error} onRetry={()=>void load()}/>}
      {!items&&!error&&<div className="admin-loading"><Spinner/> Loading organizations…</div>}
      {items&&<div className="admin-table-wrap"><table><thead><tr><th>Organization</th><th>Contact</th><th>Created</th><th>Status</th><th>Action</th></tr></thead><tbody>
        {filtered.map(o=><tr key={o.organizationId}><td><strong>{o.name}</strong><small>{o.slug}</small></td><td>{o.email||'—'}<small>{o.phone||'No phone'}</small></td><td>{new Date(o.createdAtUtc).toLocaleDateString()}</td><td><span className={`admin-status ${o.isActive?'is-active':'is-inactive'}`}>{o.isActive?'Active':'Inactive'}</span></td><td><button className={`btn btn-sm ${o.isActive?'btn-danger-soft':'btn-secondary'}`} disabled={busyId===o.organizationId} onClick={()=>void toggle(o)}>{busyId===o.organizationId?'Saving…':o.isActive?'Deactivate':'Activate'}</button></td></tr>)}
        {!filtered.length&&<tr><td colSpan={5}>No matching organizations.</td></tr>}
      </tbody></table></div>}
    </section>
  </>;
}
