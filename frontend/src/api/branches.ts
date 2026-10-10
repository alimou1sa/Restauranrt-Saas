import type { BranchResponse } from '../types/api';
import { http } from './client';
export const branchesApi = { list: () => http.get<BranchResponse[]>('/Branches').then((r) => r.data) };
