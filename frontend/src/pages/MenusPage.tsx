import { useEffect,useState,type FormEvent } from 'react';
import { branchesApi } from '../api/branches';import { productsApi } from '../api/products';
import type { BranchResponse,MenuResponse,CategoryResponse,CreateMenuRequest,CreateCategoryRequest } from '../types/api';
import { useAuth } from '../auth/AuthContext';import { PERMISSIONS } from '../auth/permissions';
import { ErrorState,Spinner } from '../components/Feedback';import { getErrorMessage } from '../utils/errors';

const emptyMenu:CreateMenuRequest={name:'',description:''};const emptyCategory:CreateCategoryRequest={name:'',description:'',displayOrder:0};
export function MenusPage(){
 const {hasPermission}=useAuth();const canManage=hasPermission(PERMISSIONS.menuManage);
 const [branches,setBranches]=useState<BranchResponse[]>([]);const [branchId,setBranchId]=useState('');
 const [menus,setMenus]=useState<MenuResponse[]>([]);const [menuId,setMenuId]=useState('');
 const [categories,setCategories]=useState<CategoryResponse[]>([]);const [error,setError]=useState<string|null>(null);
 const [menuForm,setMenuForm]=useState<CreateMenuRequest>(emptyMenu);const [categoryForm,setCategoryForm]=useState<CreateCategoryRequest>(emptyCategory);
 const [editingMenu,setEditingMenu]=useState<MenuResponse|null>(null);const [editingCategory,setEditingCategory]=useState<CategoryResponse|null>(null);const [saving,setSaving]=useState(false);
 async function loadBranches(){try{const b=await branchesApi.list();setBranches(b);if(!branchId){const first=b.find(x=>x.isActive);if(first)setBranchId(String(first.branchId))}}catch(e){setError(getErrorMessage(e))}}
 useEffect(()=>{void loadBranches()},[]);
 async function loadMenus(id:number){setError(null);try{const m=await productsApi.menus(id);setMenus(m);setMenuId(old=>m.some(x=>String(x.menuId)===old)?old:(m[0]?.menuId?String(m[0].menuId):''))}catch(e){setError(getErrorMessage(e))}}
 useEffect(()=>{if(branchId)void loadMenus(Number(branchId));else setMenus([])},[branchId]);
 async function loadCategories(id:number){try{setCategories(await productsApi.categories(id))}catch(e){setError(getErrorMessage(e))}}
 useEffect(()=>{if(menuId)void loadCategories(Number(menuId));else setCategories([])},[menuId]);
 async function saveMenu(e:FormEvent){e.preventDefault();if(!branchId)return;setSaving(true);setError(null);try{
 if(editingMenu){const updated=await productsApi.updateMenu(editingMenu.menuId,{...menuForm,isPublished:editingMenu.isPublished,isActive:editingMenu.isActive});setMenus(old=>old.map(m=>m.menuId===updated.menuId?updated:m))}
 else{const created=await productsApi.createMenu(Number(branchId),menuForm);setMenus(old=>[...old,created]);setMenuId(String(created.menuId))}
 setMenuForm(emptyMenu);setEditingMenu(null);
 }catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
 async function saveCategory(e:FormEvent){e.preventDefault();if(!menuId)return;setSaving(true);setError(null);try{
 if(editingCategory){const updated=await productsApi.updateCategory(editingCategory.categoryId,{...categoryForm,isActive:editingCategory.isActive});setCategories(old=>old.map(c=>c.categoryId===updated.categoryId?updated:c))}
 else{const created=await productsApi.createCategory(Number(menuId),categoryForm);setCategories(old=>[...old,created])}
 setCategoryForm(emptyCategory);setEditingCategory(null);
 }catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
 async function toggleMenu(m:MenuResponse,field:'isActive'|'isPublished'){try{const updated=await productsApi.updateMenu(m.menuId,{name:m.name,description:m.description,isActive:field==='isActive'?!m.isActive:m.isActive,isPublished:field==='isPublished'?!m.isPublished:m.isPublished});setMenus(old=>old.map(x=>x.menuId===updated.menuId?updated:x))}catch(e){setError(getErrorMessage(e))}}
 async function toggleCategory(c:CategoryResponse){try{const updated=await productsApi.updateCategory(c.categoryId,{name:c.name,description:c.description,displayOrder:c.displayOrder,isActive:!c.isActive});setCategories(old=>old.map(x=>x.categoryId===updated.categoryId?updated:x))}catch(e){setError(getErrorMessage(e))}}
 async function deleteMenu(m:MenuResponse){if(!window.confirm(`Delete menu "${m.name}"?`))return;try{await productsApi.removeMenu(m.menuId);setMenus(old=>old.filter(x=>x.menuId!==m.menuId));if(String(m.menuId)===menuId)setMenuId('')}catch(e){setError(getErrorMessage(e))}}
 async function deleteCategory(c:CategoryResponse){if(!window.confirm(`Delete category "${c.name}"?`))return;try{await productsApi.removeCategory(c.categoryId);setCategories(old=>old.filter(x=>x.categoryId!==c.categoryId))}catch(e){setError(getErrorMessage(e))}}
 return <>
 <div className="admin-page-heading"><div><span className="platform-eyebrow">MENU CATALOG</span><h1>Menus & categories</h1><p>Organize your restaurant catalog before adding products.</p></div><span className="admin-count">{menus.length} menus</span></div>
 {error&&<ErrorState error={error} onRetry={()=>branchId?void loadMenus(Number(branchId)):void loadBranches()}/>}
 <section className="admin-panel"><div className="admin-toolbar"><label className="toolbar-label">Branch<select value={branchId} onChange={e=>setBranchId(e.target.value)}>{branches.filter(b=>b.isActive).map(b=><option key={b.branchId} value={b.branchId}>{b.name}</option>)}</select></label><button className="btn btn-secondary" onClick={()=>branchId&&void loadMenus(Number(branchId))}>Refresh</button></div>
 <div className="admin-two-column menu-columns">
 {canManage&&<section className="admin-panel"><div className="admin-panel-heading"><div><h2>{editingMenu?'Edit menu':'Create menu'}</h2><p>Menus belong to a selected branch.</p></div>{editingMenu&&<button className="btn btn-secondary btn-sm" onClick={()=>{setEditingMenu(null);setMenuForm(emptyMenu)}}>Cancel</button>}</div>
 <form className="admin-form" onSubmit={saveMenu}><label>Menu name<input required maxLength={150} value={menuForm.name} onChange={e=>setMenuForm(f=>({...f,name:e.target.value}))}/></label><label>Description<textarea rows={2} maxLength={500} value={menuForm.description??''} onChange={e=>setMenuForm(f=>({...f,description:e.target.value}))}/></label><button className="btn btn-primary" disabled={saving||!branchId}>{saving?<Spinner/>:null}{editingMenu?'Save menu':'Create menu'}</button></form>
 <div className="simple-list">{menus.map(m=><div className="simple-list-row" key={m.menuId}><div><strong>{m.name}</strong><small>{m.isPublished?'Published':'Draft'} · {m.isActive?'Active':'Inactive'}</small></div><div className="plan-actions"><button className="btn btn-secondary btn-sm" onClick={()=>{setEditingMenu(m);setMenuForm({name:m.name,description:m.description})}}>Edit</button><button className="btn btn-secondary btn-sm" onClick={()=>void toggleMenu(m,'isPublished')}>{m.isPublished?'Unpublish':'Publish'}</button><button className="btn btn-danger-soft btn-sm" onClick={()=>void deleteMenu(m)}>Delete</button></div></div>)}{menus.length===0&&<p className="muted small">No menus for this branch yet.</p>}</div>
 </section>}
 <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Categories</h2><p>Each category belongs to a menu.</p></div></div><div className="admin-form"><label>Select menu<select value={menuId} onChange={e=>setMenuId(e.target.value)}><option value="">Choose menu…</option>{menus.map(m=><option key={m.menuId} value={m.menuId}>{m.name}</option>)}</select></label></div>
 {canManage&&menuId&&<><div className="admin-panel-heading"><div><h2>{editingCategory?'Edit category':'Add category'}</h2></div>{editingCategory&&<button className="btn btn-secondary btn-sm" onClick={()=>{setEditingCategory(null);setCategoryForm(emptyCategory)}}>Cancel</button>}</div><form className="admin-form" onSubmit={saveCategory}><label>Category name<input required maxLength={100} value={categoryForm.name} onChange={e=>setCategoryForm(f=>({...f,name:e.target.value}))}/></label><label>Description<textarea rows={2} maxLength={500} value={categoryForm.description??''} onChange={e=>setCategoryForm(f=>({...f,description:e.target.value}))}/></label><label>Display order<input type="number" value={categoryForm.displayOrder} onChange={e=>setCategoryForm(f=>({...f,displayOrder:Number(e.target.value)}))}/></label><button className="btn btn-primary" disabled={saving}>{saving?<Spinner/>:null}{editingCategory?'Save category':'Create category'}</button></form></>}
 <div className="simple-list">{categories.map(c=><div className="simple-list-row" key={c.categoryId}><div><strong>{c.name}</strong><small>Order {c.displayOrder} · {c.isActive?'Active':'Inactive'}</small></div>{canManage&&<div className="plan-actions"><button className="btn btn-secondary btn-sm" onClick={()=>{setEditingCategory(c);setCategoryForm({name:c.name,description:c.description,displayOrder:c.displayOrder})}}>Edit</button><button className="btn btn-secondary btn-sm" onClick={()=>void toggleCategory(c)}>{c.isActive?'Deactivate':'Activate'}</button><button className="btn btn-danger-soft btn-sm" onClick={()=>void deleteCategory(c)}>Delete</button></div>}</div>)}{menuId&&categories.length===0&&<p className="muted small">No categories in this menu yet.</p>}</div>
 </section>
 </div></section></>;
}
