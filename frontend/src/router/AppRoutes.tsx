import { Navigate, Route, Routes } from 'react-router-dom';
import { AppLayout } from '../layouts/AppLayout';
import { PlatformAdminLayout } from '../layouts/PlatformAdminLayout';
import { DashboardPage } from '../pages/DashboardPage';
import { LoginPage } from '../pages/LoginPage';
import { NotFoundPage } from '../pages/NotFoundPage';
import { OrganizationMembersPage } from '../pages/OrganizationMembersPage';
import { OrganizationSettingsPage } from '../pages/OrganizationSettingsPage';
import { PlatformAdminOverviewPage } from '../pages/PlatformAdminOverviewPage';
import { PlatformOrganizationsPage } from '../pages/PlatformOrganizationsPage';
import { PlatformPlansPage } from '../pages/PlatformPlansPage';
import { PlatformUsersPage } from '../pages/PlatformUsersPage';
import { RegisterPage } from '../pages/RegisterPage';
import { SelectOrganizationPage } from '../pages/SelectOrganizationPage';
import { PublicOnly, RequireAuth, RequirePlatformAdmin, RequirePreAuth } from './guards';

export function AppRoutes() {
  return <Routes>
    <Route path="/login" element={<PublicOnly><LoginPage /></PublicOnly>} />
    <Route path="/register" element={<PublicOnly><RegisterPage /></PublicOnly>} />
    <Route path="/select-organization" element={<RequirePreAuth><SelectOrganizationPage /></RequirePreAuth>} />

    <Route element={<RequirePlatformAdmin />}>
      <Route path="/platform-admin" element={<PlatformAdminLayout />}>
        <Route index element={<PlatformAdminOverviewPage />} />
        <Route path="organizations" element={<PlatformOrganizationsPage />} />
        <Route path="users" element={<PlatformUsersPage />} />
        <Route path="plans" element={<PlatformPlansPage />} />
      </Route>
    </Route>

    <Route element={<RequireAuth />}>
      <Route element={<AppLayout />}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/organization" element={<OrganizationSettingsPage />} />
        <Route path="/members" element={<OrganizationMembersPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Route>
    <Route path="/" element={<Navigate to="/dashboard" replace />} />
  </Routes>;
}
