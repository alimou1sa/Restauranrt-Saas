import { useEffect, useState, type FormEvent } from 'react';
import { organizationApi } from '../api/organization';
import type { OrganizationUserResponse, RoleResponse, CreateUserRequest } from '../types/api';
import { ErrorState, Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

const blank:CreateUserRequest={firstName:'',lastName:'',email:'',password:'',phone:''};
export function OrganizationMembersPage(){
 const [members,setMembers]=useState<OrganizationUserResponse[]|null>(null);const [roles,setRoles]=useState<RoleResponse[]>([]);
 const [loading,setLoading]=useState(true);const [error,setError]=useState<string|null>(null);const [saving,setSaving]=useState(false);
 const [form,setForm]=useState<CreateUserRequest>(blank);const [roleId,setRoleId]=useState('');const [branchId,setBranchId]=useState('');
 async function load(){setLoading(true);setError(null);try{const [m,r]=await Promise.all([organizationApi.members(),organizationApi.roles()]);setMembers(m);setRoles(r)}catch(e){setError(getErrorMessage(e))}finally{setLoading(false)}}
 useEffect(()=>{void load()},[]);
 async function add(e:FormEvent){e.preventDefault();setSaving(true);setError(null);try{
   if(!roleId)throw new Error('Choose a role before adding this member.');
   const user=await organizationApi.createUser(form);
   try{await organizationApi.addMember({userId:user.userId,branchId:branchId.trim()?Number(branchId):null,roleIds:[Number(roleId)]})}
   catch(e){throw new Error('The user account was created, but adding it to this organization failed: '+getErrorMessage(e))}
   setForm(blank);setRoleId('');setBranchId('');await load();
 }catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
 async function remove(m:OrganizationUserResponse){if(!window.confirm(`Remove ${m.userFullName} from this organization?`))return;try{await organizationApi.removeMember(m.organizationUserId);setMembers(old=>old?.filter(x=>x.organizationUserId!==m.organizationUserId)??[])}catch(e){setError(getErrorMessage(e))}}
 return <>
  <div className="admin-page-heading"><div><span className="platform-eyebrow">PEOPLE & ACCESS</span><h1>Members</h1><p>Add user accounts to this organization and assign an existing role.</p></div><span className="admin-count">{members?.length??'—'} members</span></div>
  {error&&<ErrorState error={error} onRetry={()=>void load()}/>}
  <div className="admin-two-column member-columns">
   <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Add a member</h2><p>A user account is created first, then attached to this organization.</p></div></div>
    <form className="admin-form" onSubmit={add}>
     <div className="admin-form-grid"><label>First name<input required maxLength={100} value={form.firstName} onChange={e=>setForm(f=>({...f,firstName:e.target.value}))}/></label><label>Last name<input maxLength={100} value={form.lastName??''} onChange={e=>setForm(f=>({...f,lastName:e.target.value}))}/></label></div>
     <label>Email<input required type="email" maxLength={320} value={form.email} onChange={e=>setForm(f=>({...f,email:e.target.value}))}/></label>
     <label>Temporary password<input required type="password" minLength={8} maxLength={100} autoComplete="new-password" value={form.password} onChange={e=>setForm(f=>({...f,password:e.target.value}))}/></label>
     <label>Phone (optional)<input maxLength={30} value={form.phone??''} onChange={e=>setForm(f=>({...f,phone:e.target.value}))}/></label>
     <label>Role<select required value={roleId} onChange={e=>setRoleId(e.target.value)}><option value="">Choose a role…</option>{roles.filter(r=>r.isActive).map(r=><option key={r.roleId} value={r.roleId}>{r.systemRoleName??r.name??`Role #${r.roleId}`}</option>)}</select></label>
     <label>Branch ID (optional)<input type="number" min="1" value={branchId} onChange={e=>setBranchId(e.target.value)} placeholder="Leave blank for organization-wide access"/></label>
     <button className="btn btn-primary btn-block" disabled={saving||!roles.length}>{saving?<Spinner/>:null}{saving?'Adding member…':'Create account & add member'}</button>
     <p className="muted small">The temporary password must be shared securely with the new member.</p>
    </form>
   </section>
   <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Organization members</h2><p>Memberships in the current workspace.</p></div><button className="btn btn-secondary btn-sm" onClick={()=>void load()}>Refresh</button></div>
    {loading&&!members&&<div className="admin-loading"><Spinner/> Loading members…</div>}
    {members&&<div className="admin-table-wrap"><table><thead><tr><th>Member</th><th>Branch</th><th>Status</th><th></th></tr></thead><tbody>
     {members.map(m=><tr key={m.organizationUserId}><td><strong>{m.userFullName}</strong><small>{m.userEmail}</small></td><td>{m.branchName??'All branches'}</td><td><span className={`admin-status ${m.isActive?'is-active':'is-inactive'}`}>{m.isActive?'Active':'Inactive'}</span></td><td><button className="btn btn-danger-soft btn-sm" onClick={()=>void remove(m)}>Remove</button></td></tr>)}
     {!members.length&&<tr><td colSpan={4}>No members found.</td></tr>}
    </tbody></table></div>}
   </section>
  </div>
 </>;
}
