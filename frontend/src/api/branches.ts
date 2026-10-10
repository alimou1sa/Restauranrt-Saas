import type { BranchResponse, CreateBranchRequest, UpdateBranchRequest } from '../types/api';
import { http } from './client';
export const branchesApi = {
 list: () => http.get<BranchResponse[]>('/Branches').then(r=>r.data),
 create: (body:CreateBranchRequest) => http.post<BranchResponse>('/Branches',body).then(r=>r.data),
 update: (id:number,body:UpdateBranchRequest) => http.put<BranchResponse>(`/Branches/${id}`,body).then(r=>r.data),
 remove: (id:number) => http.delete<void>(`/Branches/${id}`),
};
