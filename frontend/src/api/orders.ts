import type { OrderListResponse } from '../types/api';
import { http } from './client';
// Backend returns every order of the organization (or the token's branch) - no paging/filters yet.
export const ordersApi = { list: () => http.get<OrderListResponse[]>('/Orders').then((r) => r.data) };
