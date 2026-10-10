import { useEffect, useState, type FormEvent } from 'react';
import { platformApi } from '../api/platform';
import type { OrganizationResponse, CreateOrganizationRequest } from '../types/api';
import { ErrorState, Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

const blank:CreateOrganizationRequest={name:'',slug:'',email:'',phone:''};
export function PlatformOrganizationsPage() {
  const [items,setItems] = useState<OrganizationResponse[]|null>(null);
  const [error,setError] = useState<string|null>(null);
  const [busyId,setBusyId] = useState<number|null>(null);
  const [query,setQuery] = useState('');
  const [showCreate,setShowCreate] = useState(false);
  const [form,setForm] = useState<CreateOrganizationRequest>(blank);
  const [saving,setSaving] = useState(false);
  async function load(){setError(null);try{setItems(await platformApi.organizations())}catch(e){setError(getErrorMessage(e))}}
  useEffect(()=>{void load()},[]);
  async function toggle(item:OrganizationResponse){setBusyId(item.organizationId);try{const updated=await platformApi.setOrganizationActive(item.organizationId,{isActive:!item.isActive});setItems(old=>old?.map(x=>x.organizationId===updated.organizationId?updated:x)??[])}catch(e){setError(getErrorMessage(e))}finally{setBusyId(null)}}
  async function create(e:FormEvent){e.preventDefault();setSaving(true);setError(null);try{const created=await platformApi.createOrganization(form);setItems(old=>[created,...(old??[])]);setForm(blank);setShowCreate(false)}catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
  const filtered=(items??[]).filter(x=>[x.name,x.slug,x.email??''].some(v=>v.toLowerCase().includes(query.toLowerCase())));
  return <>
    <div className="admin-page-heading"><div><span className="platform-eyebrow">TENANT MANAGEMENT</span><h1>Organizations</h1><p>Provision customer workspaces and control whether they can access the platform.</p></div><button className="btn btn-primary" onClick={()=>setShowCreate(v=>!v)}>{showCreate?'Cancel':'＋ Create organization'}</button></div>
    {showCreate&&<section className="admin-panel"><div className="admin-panel-heading"><div><h2>Create organization</h2><p>Creates the workspace record. Initial membership and branch setup are separate steps.</p></div></div>
      <form className="admin-form admin-create-org" onSubmit={create}>
        <div className="admin-form-grid"><label>Organization name<input required maxLength={150} value={form.name} onChange={e=>setForm(f=>({...f,name:e.target.value}))}/></label><label>Unique URL slug<input required maxLength={100} pattern="[a-zA-Z0-9-]+" value={form.slug} onChange={e=>setForm(f=>({...f,slug:e.target.value.trim().toLowerCase()}))} placeholder="my-restaurant"/></label></div>
        <div className="admin-form-grid"><label>Contact email<input type="email" maxLength={320} value={form.email??''} onChange={e=>setForm(f=>({...f,email:e.target.value}))}/></label><label>Phone<input maxLength={30} value={form.phone??''} onChange={e=>setForm(f=>({...f,phone:e.target.value}))}/></label></div>
        <button className="btn btn-primary" disabled={saving}>{saving?<Spinner/>:null}{saving?'Creating…':'Create workspace'}</button>
      </form>
    </section>}
    <section className="admin-panel"><div className="admin-toolbar"><input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Search organizations…" aria-label="Search organizations" /><button className="btn btn-secondary" onClick={()=>void load()}>Refresh</button></div>
      {error&&<ErrorState error={error} onRetry={()=>void load()}/>}
      {!items&&!error&&<div className="admin-loading"><Spinner label="Loading organizations"/> Loading organizations…</div>}
      {items&&<div className="admin-table-wrap"><table><thead><tr><th>Organization</th><th>Contact</th><th>Created</th><th>Status</th><th>Action</th></tr></thead><tbody>
        {filtered.map(o=><tr key={o.organizationId}><td><strong>{o.name}</strong><small>{o.slug}</small></td><td>{o.email||'—'}<small>{o.phone||'No phone'}</small></td><td>{new Date(o.createdAtUtc).toLocaleDateString()}</td><td><span className={`admin-status ${o.isActive?'is-active':'is-inactive'}`}>{o.isActive?'Active':'Inactive'}</span></td><td><button className={`btn btn-sm ${o.isActive?'btn-danger-soft':'btn-secondary'}`} disabled={busyId===o.organizationId} onClick={()=>void toggle(o)}>{busyId===o.organizationId?'Saving…':o.isActive?'Deactivate':'Activate'}</button></td></tr>)}
        {!filtered.length&&<tr><td colSpan={5}>No matching organizations.</td></tr>}
      </tbody></table></div>}
    </section>
  </>;
}
