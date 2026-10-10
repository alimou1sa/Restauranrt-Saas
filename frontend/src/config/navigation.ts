import { PERMISSIONS, type PermissionCode } from '../auth/permissions';
import type { IconName } from '../components/Icon';

export interface NavItem {
  key: string;
  label: string;
  path: string;
  icon: IconName;
  /** Shown only if the user has this permission (UI hint; the API enforces it). */
  permission?: PermissionCode;
  /** false = the backend supports it but the page isn't built yet; shown as "Soon". Flip to true + add a route. */
  implemented: boolean;
}

export interface NavGroup { label: string; items: NavItem[] }

// Every item maps to a backend area that exists (see controllers). To add a module:
// 1) build its page + route in router/AppRoutes.tsx  2) set implemented: true here.
export const NAVIGATION: NavGroup[] = [
  {
    label: 'Operations',
    items: [
      { key: 'dashboard', label: 'Dashboard', path: '/dashboard', icon: 'dashboard', implemented: true },
      { key: 'orders', label: 'Orders', path: '/orders', icon: 'orders', permission: PERMISSIONS.orderRead, implemented: true },
      { key: 'menu', label: 'Menu', path: '/menu', icon: 'menu', permission: PERMISSIONS.menuRead, implemented: false },
      { key: 'products', label: 'Products', path: '/products', icon: 'products', permission: PERMISSIONS.productRead, implemented: true },
      { key: 'inventory', label: 'Inventory', path: '/inventory', icon: 'inventory', permission: PERMISSIONS.inventoryRead, implemented: true },
    ],
  },
  {
    label: 'Organization',
    items: [
      { key: 'branches', label: 'Branches', path: '/branches', icon: 'branches', permission: PERMISSIONS.branchRead, implemented: true },
      { key: 'users', label: 'Users', path: '/users', icon: 'users', permission: PERMISSIONS.userRead, implemented: false },
      { key: 'roles', label: 'Roles & Permissions', path: '/roles', icon: 'roles', permission: PERMISSIONS.roleRead, implemented: false },
      { key: 'members', label: 'Members', path: '/members', icon: 'users', permission: PERMISSIONS.userRead, implemented: true },
      { key: 'settings', label: 'Organization', path: '/organization', icon: 'settings', permission: PERMISSIONS.organizationRead, implemented: true },
    ],
  },
];
