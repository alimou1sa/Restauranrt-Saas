import { useEffect,useState,type FormEvent } from 'react';
import { branchesApi } from '../api/branches';
import type { BranchResponse,CreateBranchRequest } from '../types/api';
import { useAuth } from '../auth/AuthContext';
import { PERMISSIONS } from '../auth/permissions';
import { ErrorState,Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

const blank:CreateBranchRequest={name:'',address:'',phone:''};
export function BranchesPage(){
 const {hasPermission,me}=useAuth();const canManage=hasPermission(PERMISSIONS.branchManage);
 const [items,setItems]=useState<BranchResponse[]|null>(null);const [error,setError]=useState<string|null>(null);
 const [form,setForm]=useState<CreateBranchRequest>(blank);const [editing,setEditing]=useState<BranchResponse|null>(null);
 const [saving,setSaving]=useState(false);const [query,setQuery]=useState('');
 async function load(){setError(null);try{setItems(await branchesApi.list())}catch(e){setError(getErrorMessage(e))}}
 useEffect(()=>{void load()},[]);
 function edit(b:BranchResponse){setEditing(b);setForm({name:b.name,address:b.address??'',phone:b.phone??''})}
 function reset(){setEditing(null);setForm(blank)}
 async function submit(e:FormEvent){e.preventDefault();setSaving(true);setError(null);try{
  if(editing) {const updated=await branchesApi.update(editing.branchId,{...form,isActive:editing.isActive});setItems(old=>old?.map(b=>b.branchId===updated.branchId?updated:b)??[])}
  else {const created=await branchesApi.create(form);setItems(old=>[...(old??[]),created])} reset();
 }catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
 async function toggle(b:BranchResponse){try{const updated=await branchesApi.update(b.branchId,{name:b.name,address:b.address,phone:b.phone,isActive:!b.isActive});setItems(old=>old?.map(x=>x.branchId===updated.branchId?updated:x)??[])}catch(e){setError(getErrorMessage(e))}}
 async function remove(b:BranchResponse){if(!window.confirm(`Delete branch "${b.name}"? Branches with related data cannot be deleted.`))return;try{await branchesApi.remove(b.branchId);setItems(old=>old?.filter(x=>x.branchId!==b.branchId)??[])}catch(e){setError(getErrorMessage(e))}}
 const filtered=(items??[]).filter(b=>[b.name,b.address??'',b.phone??''].some(x=>x.toLowerCase().includes(query.toLowerCase())));
 return <>
 <div className="admin-page-heading"><div><span className="platform-eyebrow">ORGANIZATION</span><h1>Branches</h1><p>Manage restaurant locations, contact details and active status.</p></div><span className="admin-count">{items?.length??'—'} locations</span></div>
 {me?.branchId&&<p className="muted small">Your current session is scoped to branch #{me.branchId}. The API determines which branches you are allowed to access.</p>}
 {error&&<ErrorState error={error} onRetry={()=>void load()}/>}
 <div className="admin-two-column">
 {canManage&&<section className="admin-panel"><div className="admin-panel-heading"><div><h2>{editing?'Edit branch':'Add a branch'}</h2><p>Keep each location’s information up to date.</p></div>{editing&&<button className="btn btn-secondary btn-sm" onClick={reset}>Cancel</button>}</div>
 <form className="admin-form" onSubmit={submit}><label>Branch name<input required maxLength={150} value={form.name} onChange={e=>setForm(f=>({...f,name:e.target.value}))}/></label><label>Address<input maxLength={300} value={form.address??''} onChange={e=>setForm(f=>({...f,address:e.target.value}))}/></label><label>Phone<input maxLength={30} value={form.phone??''} onChange={e=>setForm(f=>({...f,phone:e.target.value}))}/></label><button className="btn btn-primary" disabled={saving}>{saving?<Spinner/>:null}{saving?'Saving…':editing?'Save branch':'Create branch'}</button></form>
 </section>}
 <section className="admin-panel"><div className="admin-toolbar"><input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Search branches…" aria-label="Search branches"/><button className="btn btn-secondary" onClick={()=>void load()}>Refresh</button></div>
 {!items&&!error&&<div className="admin-loading"><Spinner/> Loading branches…</div>}
 {items&&<div className="admin-table-wrap"><table><thead><tr><th>Branch</th><th>Phone</th><th>Status</th>{canManage&&<th>Actions</th>}</tr></thead><tbody>
 {filtered.map(b=><tr key={b.branchId}><td><strong>{b.name}</strong><small>{b.address||'No address'} · #{b.branchId}</small></td><td>{b.phone||'—'}</td><td><span className={`admin-status ${b.isActive?'is-active':'is-inactive'}`}>{b.isActive?'Active':'Inactive'}</span></td>{canManage&&<td><div className="plan-actions"><button className="btn btn-secondary btn-sm" onClick={()=>edit(b)}>Edit</button><button className="btn btn-secondary btn-sm" onClick={()=>void toggle(b)}>{b.isActive?'Deactivate':'Activate'}</button><button className="btn btn-danger-soft btn-sm" onClick={()=>void remove(b)}>Delete</button></div></td>}</tr>)}
 {!filtered.length&&<tr><td colSpan={canManage?4:3}>No branches found.</td></tr>}</tbody></table></div>}
 </section></div></>;
}
