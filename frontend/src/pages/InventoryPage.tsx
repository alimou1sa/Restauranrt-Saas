import { useEffect,useState,type FormEvent } from 'react';
import { branchesApi } from '../api/branches';import { inventoryApi } from '../api/inventory';import { productsApi } from '../api/products';
import type { BranchResponse,InventoryResponse,ProductListResponse,InventoryTransactionResponse } from '../types/api';
import { useAuth } from '../auth/AuthContext';import { PERMISSIONS } from '../auth/permissions';
import { ErrorState,Spinner } from '../components/Feedback';import { getErrorMessage } from '../utils/errors';
export function InventoryPage(){
 const {hasPermission,me}=useAuth();const canManage=hasPermission(PERMISSIONS.inventoryManage);
 const [branches,setBranches]=useState<BranchResponse[]>([]);const [branchId,setBranchId]=useState(me?.branchId?String(me.branchId):'');
 const [items,setItems]=useState<InventoryResponse[]|null>(null);const [products,setProducts]=useState<ProductListResponse[]>([]);
 const [transactions,setTransactions]=useState<InventoryTransactionResponse[]>([]);const [error,setError]=useState<string|null>(null);const [saving,setSaving]=useState(false);
 const [newProduct,setNewProduct]=useState('');const [initialQty,setInitialQty]=useState('0');const [reorder,setReorder]=useState('5');
 const [movementId,setMovementId]=useState('');const [movementQty,setMovementQty]=useState('1');const [movementType,setMovementType]=useState('StockIn');const [notes,setNotes]=useState('');
 async function loadBranches(){try{const b=await branchesApi.list();setBranches(b);if(!branchId){const first=b.find(x=>x.isActive);if(first)setBranchId(String(first.branchId))}}catch(e){setError(getErrorMessage(e))}}
 useEffect(()=>{void loadBranches()},[]);
 async function load(id:number){setError(null);try{const [i,p,t]=await Promise.all([inventoryApi.listByBranch(id),productsApi.listByBranch(id),inventoryApi.transactions(id)]);setItems(i);setProducts(p);setTransactions(t);if(!newProduct&&p.length)setNewProduct(String(p[0].productId));if(!movementId&&i.length)setMovementId(String(i[0].inventoryId))}catch(e){setError(getErrorMessage(e));setItems(null)}}
 useEffect(()=>{if(branchId)void load(Number(branchId));else setItems([])},[branchId]);
 async function create(e:FormEvent){e.preventDefault();if(!branchId||!newProduct)return;setSaving(true);setError(null);try{await inventoryApi.create(Number(branchId),{productId:Number(newProduct),quantity:Number(initialQty),reorderLevel:Number(reorder)});await load(Number(branchId));setInitialQty('0')}catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
 async function move(e:FormEvent){e.preventDefault();if(!movementId)return;const q=Math.abs(Number(movementQty))*(movementType==='StockOut'?-1:1);setSaving(true);setError(null);try{await inventoryApi.createTransaction(Number(movementId),{transactionType:movementType,quantity:q,notes:notes||null});await load(Number(branchId));setNotes('')}catch(e){setError(getErrorMessage(e))}finally{setSaving(false)}}
 async function saveThreshold(i:InventoryResponse){const raw=window.prompt('Set reorder threshold',String(i.reorderLevel));if(raw===null)return;const value=Number(raw);if(!Number.isFinite(value)||value<0){setError('Reorder threshold must be a non-negative number.');return}try{const updated=await inventoryApi.updateSettings(i.inventoryId,{reorderLevel:value});setItems(old=>old?.map(x=>x.inventoryId===updated.inventoryId?updated:x)??[])}catch(e){setError(getErrorMessage(e))}}
 return <>
 <div className="admin-page-heading"><div><span className="platform-eyebrow">STOCK CONTROL</span><h1>Inventory</h1><p>Monitor stock levels and record incoming or outgoing stock movements.</p></div><span className="admin-count">{items?.length??'—'} stock records</span></div>
 {error&&<ErrorState error={error} onRetry={()=>branchId?void load(Number(branchId)):void loadBranches()}/>}
 <section className="admin-panel"><div className="admin-toolbar"><label className="toolbar-label">Branch<select value={branchId} onChange={e=>setBranchId(e.target.value)}>{branches.filter(b=>b.isActive).map(b=><option key={b.branchId} value={b.branchId}>{b.name}</option>)}</select></label><button className="btn btn-secondary" onClick={()=>branchId&&void load(Number(branchId))}>Refresh</button></div>
 {!items&&!error&&<div className="admin-loading"><Spinner/> Loading inventory…</div>}
 {items&&<div className="admin-table-wrap"><table><thead><tr><th>Product</th><th>Quantity</th><th>Reorder level</th><th>Stock health</th>{canManage&&<th>Action</th>}</tr></thead><tbody>
 {items.map(i=><tr key={i.inventoryId}><td><strong>{i.productName}</strong><small>Inventory #{i.inventoryId}</small></td><td><strong>{i.quantity}</strong></td><td>{i.reorderLevel}</td><td><span className={`admin-status ${i.isLowStock?'is-inactive':'is-active'}`}>{i.isLowStock?'Low stock':'In stock'}</span></td>{canManage&&<td><button className="btn btn-secondary btn-sm" onClick={()=>void saveThreshold(i)}>Edit threshold</button></td>}</tr>)}
 {!items.length&&<tr><td colSpan={canManage?5:4}>No stock records for this branch yet.</td></tr>}</tbody></table></div>}
 </section>
 {canManage&&<div className="admin-two-column">
 <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Initialize stock</h2><p>Create a stock record for a product at this branch.</p></div></div><form className="admin-form" onSubmit={create}>
 <label>Product<select required value={newProduct} onChange={e=>setNewProduct(e.target.value)}><option value="">Choose product…</option>{products.map(p=><option key={p.productId} value={p.productId}>{p.name}</option>)}</select></label>
 <div className="admin-form-grid"><label>Opening quantity<input required type="number" min="0" step="0.01" value={initialQty} onChange={e=>setInitialQty(e.target.value)}/></label><label>Low-stock threshold<input required type="number" min="0" step="0.01" value={reorder} onChange={e=>setReorder(e.target.value)}/></label></div>
 <button className="btn btn-primary" disabled={saving||!products.length||!branchId}>{saving?<Spinner/>:null}Create stock record</button>
 </form></section>
 <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Record stock movement</h2><p>All quantity changes are recorded in the movement history.</p></div></div><form className="admin-form" onSubmit={move}>
 <label>Stock record<select required value={movementId} onChange={e=>setMovementId(e.target.value)}><option value="">Choose stock record…</option>{items?.map(i=><option key={i.inventoryId} value={i.inventoryId}>{i.productName} · available {i.quantity}</option>)}</select></label>
 <label>Movement type<select value={movementType} onChange={e=>setMovementType(e.target.value)}><option value="StockIn">Stock in</option><option value="StockOut">Stock out</option><option value="Adjustment">Adjustment (adds quantity)</option></select></label>
 <label>Quantity<input required type="number" min="0.01" step="0.01" value={movementQty} onChange={e=>setMovementQty(e.target.value)}/></label><label>Notes (optional)<textarea rows={2} maxLength={500} value={notes} onChange={e=>setNotes(e.target.value)}/></label>
 <button className="btn btn-primary" disabled={saving||!items?.length}>{saving?<Spinner/>:null}Record movement</button>
 </form></section></div>}
 <section className="admin-panel"><div className="admin-panel-heading"><div><h2>Recent stock movements</h2><p>Audit history for the selected branch.</p></div></div><div className="admin-table-wrap"><table><thead><tr><th>When</th><th>Product</th><th>Movement</th><th>Quantity change</th><th>Notes</th></tr></thead><tbody>{transactions.slice(0,20).map(t=><tr key={t.inventoryTransactionId}><td>{new Date(t.createdAtUtc).toLocaleString()}</td><td><strong>{t.productName}</strong></td><td>{t.transactionType}</td><td>{t.quantity}</td><td>{t.notes||'—'}</td></tr>)}{transactions.length===0&&<tr><td colSpan={5}>No stock movements recorded yet.</td></tr>}</tbody></table></div></section>
 </>;
}
