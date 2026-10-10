import { useEffect, useState } from 'react';
import { platformApi } from '../api/platform';
import type { UserResponse } from '../types/api';
import { ErrorState, Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

export function PlatformUsersPage() {
  const [items,setItems]=useState<UserResponse[]|null>(null); const [error,setError]=useState<string|null>(null);
  const [busyId,setBusyId]=useState<number|null>(null); const [query,setQuery]=useState('');
  async function load(){setError(null);try{setItems(await platformApi.users())}catch(e){setError(getErrorMessage(e))}}
  useEffect(()=>{void load()},[]);
  async function toggle(item:UserResponse){setBusyId(item.userId);try{const updated=await platformApi.setUserActive(item.userId,{isActive:!item.isActive});setItems(old=>old?.map(x=>x.userId===updated.userId?updated:x)??[])}catch(e){setError(getErrorMessage(e))}finally{setBusyId(null)}}
  const filtered=(items??[]).filter(x=>[x.firstName,x.lastName??'',x.email].some(v=>v.toLowerCase().includes(query.toLowerCase())));
  return <>
    <div className="admin-page-heading"><div><span className="platform-eyebrow">ACCOUNT MANAGEMENT</span><h1>Users</h1><p>Review platform accounts and enable or disable access when needed.</p></div><span className="admin-count">{items?.length??'—'} total</span></div>
    <section className="admin-panel"><div className="admin-toolbar"><input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Search by name or email…" aria-label="Search users"/><button className="btn btn-secondary" onClick={()=>void load()}>Refresh</button></div>
      {error&&<ErrorState error={error} onRetry={()=>void load()}/>}
      {!items&&!error&&<div className="admin-loading"><Spinner/> Loading users…</div>}
      {items&&<div className="admin-table-wrap"><table><thead><tr><th>User</th><th>Email verification</th><th>Last login</th><th>Status</th><th>Action</th></tr></thead><tbody>
        {filtered.map(u=><tr key={u.userId}><td><strong>{u.firstName} {u.lastName??''}</strong><small>Account #{u.userId}</small></td><td>{u.emailConfirmed?'Confirmed':'Not confirmed'}</td><td>{u.lastLoginAtUtc?new Date(u.lastLoginAtUtc).toLocaleString():'Never'}</td><td><span className={`admin-status ${u.isActive?'is-active':'is-inactive'}`}>{u.isActive?'Active':'Inactive'}</span></td><td><button className={`btn btn-sm ${u.isActive?'btn-danger-soft':'btn-secondary'}`} disabled={busyId===u.userId} onClick={()=>void toggle(u)}>{busyId===u.userId?'Saving…':u.isActive?'Deactivate':'Activate'}</button></td></tr>)}
        {!filtered.length&&<tr><td colSpan={5}>No matching users.</td></tr>}
      </tbody></table></div>}
    </section>
  </>;
}
