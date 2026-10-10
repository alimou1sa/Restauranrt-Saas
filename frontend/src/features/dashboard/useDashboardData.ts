import { useMemo } from 'react';
import { branchesApi } from '../../api/branches';
import { inventoryApi } from '../../api/inventory';
import { ordersApi } from '../../api/orders';
import { productsApi } from '../../api/products';
import { useAuth } from '../../auth/AuthContext';
import { PERMISSIONS } from '../../auth/permissions';
import { useAsync } from '../../hooks/useAsync';
import { isToday, parseUtc } from '../../utils/format';

const IN_PROGRESS = ['Confirmed', 'Preparing', 'Ready'];

/**
 * All numbers come from existing endpoints. The backend has no analytics endpoint, so orders are
 * aggregated in the browser from GET /api/Orders (see README "Backend gaps").
 */
export function useDashboardData() {
  const { me, hasPermission } = useAuth();
  const key = me?.organizationUserId;

  const canOrders = hasPermission(PERMISSIONS.orderRead);
  const canProducts = hasPermission(PERMISSIONS.productRead);
  const canInventory = hasPermission(PERMISSIONS.inventoryRead);
  const canBranches = hasPermission(PERMISSIONS.branchRead);
  const needsBranches = canProducts || canInventory;

  const orders = useAsync(() => ordersApi.list(), [key], canOrders);

  // Products and inventory are per-branch endpoints. A branch-bound token has exactly one branch;
  // an organization-wide token must list the branches first (needs branch.read).
  const branchIds = useAsync(
    async () => {
      if (me?.branchId != null) return [me.branchId];
      if (!canBranches) throw new Error('Listing branches requires the branch.read permission.');
      return (await branchesApi.list()).filter((b) => b.isActive).map((b) => b.branchId);
    },
    [key],
    needsBranches,
  );

  const ids = branchIds.data;
  const products = useAsync(
    async () => (await Promise.all((ids ?? []).map(productsApi.listByBranch))).flat(),
    [key, ids],
    canProducts && !!ids,
  );
  const inventory = useAsync(
    async () => (await Promise.all((ids ?? []).map(inventoryApi.listByBranch))).flat(),
    [key, ids],
    canInventory && !!ids,
  );

  const orderStats = useMemo(() => {
    const list = orders.data ?? [];
    const today = list.filter((o) => isToday(o.createdAtUtc));
    return {
      todayOrders: today.filter((o) => o.status !== 'Canceled').length,
      todayRevenue: today.filter((o) => o.status === 'Completed').reduce((sum, o) => sum + o.totalAmount, 0),
      pending: list.filter((o) => o.status === 'Pending').length,
      inProgress: list.filter((o) => IN_PROGRESS.includes(o.status)).length,
      recent: [...list]
        .sort((a, b) => parseUtc(b.createdAtUtc).getTime() - parseUtc(a.createdAtUtc).getTime())
        .slice(0, 5),
    };
  }, [orders.data]);

  const catalogLoading = branchIds.loading;
  const reloadAll = () => {
    orders.reload();
    branchIds.reload();
    products.reload();
    inventory.reload();
  };

  return {
    canOrders, canProducts, canInventory,
    orders, orderStats,
    products: { ...products, loading: catalogLoading || products.loading, error: branchIds.error ?? products.error },
    inventory: { ...inventory, loading: catalogLoading || inventory.loading, error: branchIds.error ?? inventory.error },
    branchCount: ids?.length ?? null,
    reloadAll,
  };
}
