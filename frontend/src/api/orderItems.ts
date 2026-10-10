import type { OrderItemResponse,CreateOrderItemRequest,UpdateOrderItemRequest } from '../types/api';
import { http } from './client';
export const orderItemsApi = {
 list: (orderId:number) => http.get<OrderItemResponse[]>(`/OrderItems/orders/${orderId}`).then(r=>r.data),
 add: (orderId:number,body:CreateOrderItemRequest) => http.post<OrderItemResponse>(`/OrderItems/orders/${orderId}`,body).then(r=>r.data),
 update: (id:number,body:UpdateOrderItemRequest) => http.put<OrderItemResponse>(`/OrderItems/${id}`,body).then(r=>r.data),
 remove: (id:number) => http.delete<void>(`/OrderItems/${id}`),
};
