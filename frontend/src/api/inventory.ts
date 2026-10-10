import type { InventoryResponse } from '../types/api';
import { http } from './client';
export const inventoryApi = {
  listByBranch: (branchId: number) =>
    http.get<InventoryResponse[]>(`/branches/${branchId}/inventories`).then((r) => r.data),
};
