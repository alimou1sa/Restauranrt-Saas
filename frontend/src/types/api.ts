// Mirrors the backend DTOs (RestaurantSaaS.Application.DTOs). ASP.NET Core serializes camelCase.

export interface LoginRequest { email: string; password: string }

export interface OrganizationOption {
  organizationId: number;
  organizationName: string;
  branchId: number | null;
  branchName: string | null;
}

export interface LoginResponse {
  token: string;
  tokenType: string; // "org_selection" for the PreAuth token
  organizations: OrganizationOption[];
}

export interface AuthTokenResponse {
  token: string;
  refreshToken: string;
  organizationId: number;
  organizationUserId: number;
  branchId: number | null;
}

// GET /api/auth/me - requires the backend patch in backend-patch/BACKEND_PATCH.md
export interface MeResponse {
  userId: number;
  email: string;
  firstName: string;
  lastName: string | null;
  organizationId: number;
  organizationName: string;
  organizationUserId: number;
  branchId: number | null;
  branchName: string | null;
  permissions: string[];
}

export interface BranchResponse {
  branchId: number;
  name: string;
  address: string | null;
  phone: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export type OrderStatus = 'Pending' | 'Confirmed' | 'Preparing' | 'Ready' | 'Served' | 'Completed' | 'Canceled';

export interface OrderListResponse {
  orderId: number;
  orderNumber: string;
  branchName: string;
  tableName: string | null;
  customerName: string | null;
  status: OrderStatus | string;
  orderType: string;
  totalAmount: number;
  createdAtUtc: string;
}

export interface ProductListResponse {
  productId: number;
  name: string;
  categoryId: number;
  categoryName: string;
  price: number;
  imageUrl: string | null;
  isAvailable: boolean;
  displayOrder: number;
}

export interface InventoryResponse {
  inventoryId: number;
  branchId: number;
  productId: number;
  productName: string;
  quantity: number;
  reorderLevel: number;
  isLowStock: boolean;
  updatedAtUtc: string;
}
