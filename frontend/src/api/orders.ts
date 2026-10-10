import type { OrderListResponse, OrderDetailsResponse, CreateOrderRequest, UpdateOrderStatusRequest, BranchResponse } from '../types/api';
import { http } from './client';
export const ordersApi = {
 list: () => http.get<OrderListResponse[]>('/Orders').then(r=>r.data),
 get: (id:number) => http.get<OrderDetailsResponse>(`/Orders/${id}`).then(r=>r.data),
 create: (branchId:number,body:CreateOrderRequest) => http.post<OrderDetailsResponse>(`/Orders/branches/${branchId}`,body).then(r=>r.data),
 updateStatus: (id:number,body:UpdateOrderStatusRequest) => http.patch<OrderDetailsResponse>(`/Orders/${id}/status`,body).then(r=>r.data),
 remove: (id:number) => http.delete<void>(`/Orders/${id}`),
};
