import { useEffect, useState, type FormEvent } from 'react';
import { platformApi } from '../api/platform';
import type { PlanResponse, CreatePlanRequest } from '../types/api';
import { ErrorState, Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

const emptyForm:CreatePlanRequest={name:'',description:'',monthlyPrice:0,yearlyPrice:0,maxBranches:1,maxUsers:5,maxProducts:50};
export function PlatformPlansPage(){
 const [items,setItems]=useState<PlanResponse[]|null>(null);const [error,setError]=useState<string|null>(null);
 const [form,setForm]=useState<CreatePlanRequest>(emptyForm);const [editing,setEditing]=useState<PlanResponse|null>(null);const [saving,setSaving]=useState(false);
 async function load(){setError(null);try{setItems(await platformApi.plans())}catch(e){setError(getErrorMessage(e))}}
 useEffect(()=>{void load()},[]);
 function startEdit(p:PlanResponse){setEditing(p);setForm({name:p.name,description:p.description,monthlyPrice:p.monthlyPrice,yearlyPrice:p.yearlyPrice,maxBranches:p.maxBranches,maxUsers:p.maxUsers,maxProducts:p.maxProducts})}
 function reset(){setEditing(null);setForm(emptyForm)}
 async function submit(e:FormEvent){e.preventDefault();setSaving(true);setError(null);try{
   if(editing){const updated=await platformApi.updatePlan(editing.planId,{...form,isActive:editing.isActive});setItems(old=>old?.map(p=>p.planId===updated.planId?updated:p)??[])}
   else{const created=await platformApi.createPlan(form);setItems(old=>[...(old??[]),created])}reset();
 }catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
 async function remove(p:PlanResponse){if(!window.confirm(`Delete the plan "${p.name}"? This cannot be undone.`))return;try{await platformApi.deletePlan(p.planId);setItems(old=>old?.filter(x=>x.planId!==p.planId)??[])}catch(e){setError(getErrorMessage(e))}}
 const setText=(key:'name'|'description',value:string)=>setForm(old=>({...old,[key]:value}));
 const setNumber=(key:'monthlyPrice'|'yearlyPrice'|'maxBranches'|'maxUsers'|'maxProducts',value:string)=>{
   const numeric=value===''?null:Number(value);
   setForm(old=>({...old,[key]:numeric===null?(key==='monthlyPrice'||key==='yearlyPrice'?0:null):numeric}));
 };
 return <>
  <div className="admin-page-heading"><div><span className="platform-eyebrow">BILLING CONFIGURATION</span><h1>Plans & pricing</h1><p>Configure the plan catalog used by the platform. This does not process payments.</p></div></div>
  {error&&<ErrorState error={error} onRetry={()=>void load()}/>}
  <div className="admin-two-column">
   <section className="admin-panel"><div className="admin-panel-heading"><div><h2>{editing?'Edit plan':'Create a plan'}</h2><p>Set prices and usage limits.</p></div>{editing&&<button className="btn btn-secondary btn-sm" onClick={reset}>Cancel</button>}</div>
    <form className="admin-form" onSubmit={submit}>
      <label>Plan name<input required maxLength={100} value={form.name} onChange={e=>setText('name',e.target.value)}/></label>
      <label>Description<textarea rows={3} maxLength={500} value={form.description??''} onChange={e=>setText('description',e.target.value)}/></label>
      <div className="admin-form-grid"><label>Monthly price<input type="number" min="0" step="0.01" required value={form.monthlyPrice} onChange={e=>setNumber('monthlyPrice',e.target.value)}/></label><label>Yearly price<input type="number" min="0" step="0.01" required value={form.yearlyPrice} onChange={e=>setNumber('yearlyPrice',e.target.value)}/></label></div>
      <div className="admin-form-grid"><label>Max branches<input type="number" min="1" value={form.maxBranches??''} onChange={e=>setNumber('maxBranches',e.target.value)}/></label><label>Max users<input type="number" min="1" value={form.maxUsers??''} onChange={e=>setNumber('maxUsers',e.target.value)}/></label></div>
      <label>Max products<input type="number" min="1" value={form.maxProducts??''} onChange={e=>setNumber('maxProducts',e.target.value)}/></label>
      <button className="btn btn-primary btn-block" disabled={saving}>{saving?<Spinner label="Saving"/>:null}{saving?'Saving…':editing?'Save changes':'Create plan'}</button>
    </form>
   </section>
   <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Available plans</h2><p>{items?.length??0} configured plans</p></div><button className="btn btn-secondary btn-sm" onClick={()=>void load()}>Refresh</button></div>
    {!items&&!error&&<div className="admin-loading"><Spinner/> Loading plans…</div>}
    {items?.map(p=><article className="plan-row" key={p.planId}><div><div className="plan-row-title"><strong>{p.name}</strong><span className={`admin-status ${p.isActive?'is-active':'is-inactive'}`}>{p.isActive?'Active':'Inactive'}</span></div><p>{p.description||'No description'}</p><div className="plan-prices"><strong>{p.monthlyPrice.toLocaleString()} <small>/ month</small></strong><span>{p.yearlyPrice.toLocaleString()} / year</span></div><small>{p.maxBranches??'Unlimited'} branches · {p.maxUsers??'Unlimited'} users · {p.maxProducts??'Unlimited'} products</small></div><div className="plan-actions"><button className="btn btn-secondary btn-sm" onClick={()=>startEdit(p)}>Edit</button><button className="btn btn-danger-soft btn-sm" onClick={()=>void remove(p)}>Delete</button></div></article>)}
    {items?.length===0&&<p className="muted">No plans yet. Create the first one.</p>}
   </section>
  </div>
 </>;
}
