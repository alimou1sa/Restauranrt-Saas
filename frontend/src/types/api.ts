// Mirrors the backend DTOs. ASP.NET Core serializes properties as camelCase.
export interface LoginRequest { email: string; password: string }

export interface OrganizationOption {
  organizationId: number;
  organizationName: string;
  branchId: number | null;
  branchName: string | null;
}

export interface LoginResponse {
  token: string;
  tokenType: string; // "org_selection" or "platform_admin"
  organizations: OrganizationOption[];
}

export interface AuthTokenResponse {
  token: string;
  refreshToken: string;
  organizationId: number;
  organizationUserId: number;
  branchId: number | null;
}

export interface MeResponse {
  userId: number;
  email: string;
  firstName: string;
  lastName: string | null;
  isPlatformAdmin: boolean;
  organizationId?: number | null;
  organizationName?: string | null;
  organizationUserId?: number | null;
  branchId?: number | null;
  branchName?: string | null;
  roles?: string[];
  permissions: string[];
}

export interface OrganizationResponse {
  organizationId: number;
  name: string;
  slug: string;
  email: string | null;
  phone: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}
export interface CreateOrganizationRequest { name: string; slug: string; email?: string | null; phone?: string | null }
export interface UpdateOrganizationRequest { name: string; email?: string | null; phone?: string | null; isActive: boolean }

export interface UserResponse {
  userId: number;
  firstName: string;
  lastName: string | null;
  email: string;
  phone: string | null;
  isActive: boolean;
  emailConfirmed: boolean;
  lastLoginAtUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}
export interface CreateUserRequest { firstName: string; lastName?: string | null; email: string; password: string; phone?: string | null }
export interface SetActiveRequest { isActive: boolean }

export interface OrganizationUserResponse {
  organizationUserId: number;
  organizationId: number;
  userId: number;
  userFullName: string;
  userEmail: string;
  branchId: number | null;
  branchName: string | null;
  isActive: boolean;
  joinedAtUtc: string;
  removedAtUtc: string | null;
}
export interface RoleResponse {
  roleId: number;
  organizationId: number;
  name: string | null;
  description: string | null;
  systemRoleId: number | null;
  systemRoleName: string | null;
  isActive: boolean;
  createdAtUtc: string;
}
export interface AddMemberWithRolesRequest { userId: number; branchId: number | null; roleIds: number[] }

export interface PlanResponse {
  planId: number;
  name: string;
  description: string | null;
  monthlyPrice: number;
  yearlyPrice: number;
  maxBranches: number | null;
  maxUsers: number | null;
  maxProducts: number | null;
  isActive: boolean;
  createdAtUtc: string;
}
export interface CreatePlanRequest {
  name: string; description?: string | null; monthlyPrice: number; yearlyPrice: number;
  maxBranches?: number | null; maxUsers?: number | null; maxProducts?: number | null;
}
export interface UpdatePlanRequest extends CreatePlanRequest { isActive: boolean }

export interface BranchResponse {
  branchId: number; name: string; address: string | null; phone: string | null; isActive: boolean;
  createdAtUtc: string; updatedAtUtc: string | null;
}
export type OrderStatus = 'Pending' | 'Confirmed' | 'Preparing' | 'Ready' | 'Served' | 'Completed' | 'Canceled';
export interface OrderListResponse {
  orderId: number; orderNumber: string; branchName: string; tableName: string | null; customerName: string | null;
  status: OrderStatus | string; orderType: string; totalAmount: number; createdAtUtc: string;
}
export interface ProductListResponse {
  productId: number; name: string; categoryId: number; categoryName: string; price: number;
  imageUrl: string | null; isAvailable: boolean; displayOrder: number;
}
export interface InventoryResponse {
  inventoryId: number; branchId: number; productId: number; productName: string; quantity: number;
  reorderLevel: number; isLowStock: boolean; updatedAtUtc: string;
}
