import { http } from './client';
import type { OrganizationResponse, UpdateOrganizationRequest, OrganizationUserResponse, RoleResponse, UserResponse, CreateUserRequest, AddMemberWithRolesRequest } from '../types/api';

export const organizationApi = {
  current: () => http.get<OrganizationResponse>('/organizations/current').then(r => r.data),
  updateCurrent: (body: UpdateOrganizationRequest) => http.put<OrganizationResponse>('/organizations/current', body).then(r => r.data),
  members: () => http.get<OrganizationUserResponse[]>('/OrganizationUsers').then(r => r.data),
  roles: () => http.get<RoleResponse[]>('/roles').then(r => r.data),
  createUser: (body: CreateUserRequest) => http.post<UserResponse>('/users', body).then(r => r.data),
  addMember: (body: AddMemberWithRolesRequest) => http.post<OrganizationUserResponse>('/OrganizationUsers/with-roles', body).then(r => r.data),
  removeMember: (id: number) => http.delete<void>(`/OrganizationUsers/${id}`),
};
