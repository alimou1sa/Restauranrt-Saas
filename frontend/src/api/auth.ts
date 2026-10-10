import type { AuthTokenResponse, LoginRequest, LoginResponse, MeResponse, OrganizationOption } from '../types/api';
import { http } from './client';

export const authApi = {
  login: (body: LoginRequest) =>
    http.post<LoginResponse>('/auth/login', body, { authMode: 'none' }).then((r) => r.data),

  // Both accept the PreAuth token (policy "AuthenticatedAny").
  myOrganizations: () => http.get<OrganizationOption[]>('/auth', { authMode: 'preauth' }).then((r) => r.data),
  selectOrganization: (organizationId: number) =>
    http
      .post<AuthTokenResponse>('/auth/select-organization', { organizationId }, { authMode: 'preauth' })
      .then((r) => r.data),

  me: () => http.get<MeResponse>('/auth/me').then((r) => r.data),

  logout: (refreshToken: string) => http.post<void>('/auth/logout', { refreshToken }),
};
