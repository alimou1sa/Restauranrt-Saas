import { http } from './client';
import type {
  OrganizationResponse, UserResponse, SetActiveRequest, PlanResponse, CreatePlanRequest, UpdatePlanRequest,
} from '../types/api';

export const platformApi = {
  organizations: () => http.get<OrganizationResponse[]>('/platform/organizations').then(r => r.data),
  createOrganization: (body: import('../types/api').CreateOrganizationRequest) => http.post<OrganizationResponse>('/organizations', body).then(r => r.data),
  organization: (id: number) => http.get<OrganizationResponse>(`/platform/organizations/${id}`).then(r => r.data),
  setOrganizationActive: (id: number, body: SetActiveRequest) =>
    http.patch<OrganizationResponse>(`/platform/organizations/${id}/active`, body).then(r => r.data),
  users: () => http.get<UserResponse[]>('/platform/users').then(r => r.data),
  user: (id: number) => http.get<UserResponse>(`/platform/users/${id}`).then(r => r.data),
  setUserActive: (id: number, body: SetActiveRequest) =>
    http.patch<UserResponse>(`/platform/users/${id}/active`, body).then(r => r.data),
  plans: () => http.get<PlanResponse[]>('/platform/plans').then(r => r.data),
  createPlan: (body: CreatePlanRequest) => http.post<PlanResponse>('/platform/plans', body).then(r => r.data),
  updatePlan: (id: number, body: UpdatePlanRequest) => http.put<PlanResponse>(`/platform/plans/${id}`, body).then(r => r.data),
  deletePlan: (id: number) => http.delete<void>(`/platform/plans/${id}`),
};
