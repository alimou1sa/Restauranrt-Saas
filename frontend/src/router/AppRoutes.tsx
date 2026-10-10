import { Navigate, Route, Routes } from 'react-router-dom';
import { AppLayout } from '../layouts/AppLayout';
import { DashboardPage } from '../pages/DashboardPage';
import { LoginPage } from '../pages/LoginPage';
import { NotFoundPage } from '../pages/NotFoundPage';
import { SelectOrganizationPage } from '../pages/SelectOrganizationPage';
import { PublicOnly, RequireAuth, RequirePreAuth } from './guards';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<PublicOnly><LoginPage /></PublicOnly>} />
      <Route path="/select-organization" element={<RequirePreAuth><SelectOrganizationPage /></RequirePreAuth>} />

      <Route element={<RequireAuth />}>
        <Route element={<AppLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          {/* Next milestones: <Route path="/orders" element={<RequirePermission code="order.read"><OrdersPage/></RequirePermission>} /> */}
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Route>

      <Route path="/" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}
