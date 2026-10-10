import type { ProductListResponse } from '../types/api';
import { http } from './client';
export const productsApi = {
  listByBranch: (branchId: number) =>
    http.get<ProductListResponse[]>(`/Products/branches/${branchId}`).then((r) => r.data),
};
