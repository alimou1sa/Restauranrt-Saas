import type { InventoryResponse, CreateInventoryRequest, UpdateInventorySettingsRequest, CreateInventoryTransactionRequest, InventoryTransactionResponse } from '../types/api';
import { http } from './client';
export const inventoryApi = {
 listByBranch: (branchId:number) => http.get<InventoryResponse[]>(`/branches/${branchId}/inventories`).then(r=>r.data),
 create: (branchId:number,body:CreateInventoryRequest) => http.post<InventoryResponse>(`/branches/${branchId}/inventories`,body).then(r=>r.data),
 updateSettings: (id:number,body:UpdateInventorySettingsRequest) => http.put<InventoryResponse>(`/inventories/${id}/settings`,body).then(r=>r.data),
 transactions: (branchId:number) => http.get<InventoryTransactionResponse[]>(`/InventoryTransactions/branches/${branchId}`).then(r=>r.data),
 createTransaction: (inventoryId:number,body:CreateInventoryTransactionRequest) => http.post<InventoryTransactionResponse>(`/InventoryTransactions/inventories/${inventoryId}`,body).then(r=>r.data),
 remove: (id:number) => http.delete<void>(`/inventories/${id}`),
};
