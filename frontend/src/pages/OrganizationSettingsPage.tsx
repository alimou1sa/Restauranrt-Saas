import { useEffect, useState, type FormEvent } from 'react';
import { organizationApi } from '../api/organization';
import type { OrganizationResponse, UpdateOrganizationRequest } from '../types/api';
import { ErrorState, Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

export function OrganizationSettingsPage(){
 const [org,setOrg]=useState<OrganizationResponse|null>(null);const [form,setForm]=useState<UpdateOrganizationRequest>({name:'',email:'',phone:'',isActive:true});
 const [loading,setLoading]=useState(true);const [saving,setSaving]=useState(false);const [error,setError]=useState<string|null>(null);const [success,setSuccess]=useState(false);
 async function load(){setLoading(true);setError(null);try{const x=await organizationApi.current();setOrg(x);setForm({name:x.name,email:x.email??'',phone:x.phone??'',isActive:x.isActive})}catch(e){setError(getErrorMessage(e))}finally{setLoading(false)}}
 useEffect(()=>{void load()},[]);
 async function submit(e:FormEvent){e.preventDefault();setSaving(true);setError(null);setSuccess(false);try{const x=await organizationApi.updateCurrent(form);setOrg(x);setForm({name:x.name,email:x.email??'',phone:x.phone??'',isActive:x.isActive});setSuccess(true)}catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
 return <>
  <div className="admin-page-heading"><div><span className="platform-eyebrow">WORKSPACE SETTINGS</span><h1>Organization</h1><p>Update the business details for the currently selected organization.</p></div></div>
  {error&&<ErrorState error={error} onRetry={()=>void load()}/>}
  {loading&&<div className="admin-loading"><Spinner/> Loading organization…</div>}
  {!loading&&org&&<div className="admin-two-column">
   <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Business details</h2><p>Changes apply to this workspace.</p></div></div>
    {success&&<div className="admin-success" role="status">Organization details saved successfully.</div>}
    <form className="admin-form" onSubmit={submit}>
     <label>Organization name<input required maxLength={150} value={form.name} onChange={e=>setForm(f=>({...f,name:e.target.value}))}/></label>
     <label>Contact email<input type="email" maxLength={320} value={form.email??''} onChange={e=>setForm(f=>({...f,email:e.target.value}))}/></label>
     <label>Phone number<input maxLength={30} value={form.phone??''} onChange={e=>setForm(f=>({...f,phone:e.target.value}))}/></label>
     <label className="admin-checkbox"><input type="checkbox" checked={form.isActive} onChange={e=>setForm(f=>({...f,isActive:e.target.checked}))}/> Organization is active</label>
     <button className="btn btn-primary" disabled={saving}>{saving?<Spinner/>:null}{saving?'Saving…':'Save organization'}</button>
    </form>
   </section>
   <aside className="admin-panel org-summary"><span className="platform-eyebrow">WORKSPACE ID</span><strong>#{org.organizationId}</strong><p className="muted">This identifier is managed by the platform and cannot be changed.</p><div className="org-summary-line"><span>URL slug</span><strong>{org.slug}</strong></div><div className="org-summary-line"><span>Created</span><strong>{new Date(org.createdAtUtc).toLocaleDateString()}</strong></div><div className="org-summary-line"><span>Status</span><span className={`admin-status ${org.isActive?'is-active':'is-inactive'}`}>{org.isActive?'Active':'Inactive'}</span></div></aside>
  </div>}
 </>;
}
