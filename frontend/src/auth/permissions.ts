// Permission codes enforced by the backend via [HasPermission("...")].
// Only codes that appear in the controllers are listed here.
export const PERMISSIONS = {
  organizationRead: 'organization.read',
  organizationUpdate: 'organization.update',
  branchRead: 'branch.read',
  branchManage: 'branch.manage',
  userRead: 'user.read',
  userManage: 'user.manage',
  roleRead: 'role.read',
  roleManage: 'role.manage',
  menuRead: 'menu.read',
  menuManage: 'menu.manage',
  productRead: 'product.read',
  productManage: 'product.manage',
  orderRead: 'order.read',
  orderManage: 'order.manage',
  paymentRead: 'payment.read',
  paymentManage: 'payment.manage',
  customerRead: 'customer.read',
  customerManage: 'customer.manage',
  inventoryRead: 'inventory.read',
  inventoryManage: 'inventory.manage',
  tableRead: 'table.read',
  tableManage: 'table.manage',
  subscriptionRead: 'subscription.read',
  subscriptionManage: 'subscription.manage',
} as const;

export type PermissionCode = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
