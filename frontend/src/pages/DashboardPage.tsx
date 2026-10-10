import { useAuth } from '../auth/AuthContext';
import { PERMISSIONS } from '../auth/permissions';
import { Icon } from '../components/Icon';
import { useToast } from '../components/Toast';
import { RecentOrders } from '../features/dashboard/RecentOrders';
import { StatCard } from '../features/dashboard/StatCard';
import { useDashboardData } from '../features/dashboard/useDashboardData';
import { formatAmount, formatCount } from '../utils/format';

export function DashboardPage() {
  const { me } = useAuth();
  const toast = useToast();
  const d = useDashboardData();
  if (!me) return null;

  const products = d.products.data;
  const lowStock = d.inventory.data?.filter((i) => i.isLowStock).length ?? 0;

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="h1">Dashboard</h1>
          <p className="muted">{me.organizationName} · {me.branchName ?? 'All branches'}</p>
        </div>
        <button type="button" className="btn btn-secondary btn-sm"
          onClick={() => { d.reloadAll(); toast.info('Refreshing dashboard…'); }}>
          <Icon name="refresh" size={16} /> Refresh
        </button>
      </div>

      <div className="grid-stats">
        <StatCard label="Today's orders" value={d.canOrders ? formatCount(d.orderStats.todayOrders) : undefined}
          hint="Created today, excluding canceled" loading={d.orders.loading} error={d.orders.error}
          lockedBy={d.canOrders ? undefined : PERMISSIONS.orderRead} onRetry={d.orders.reload} />
        <StatCard label="Today's revenue" value={formatAmount(d.orderStats.todayRevenue)}
          hint="Completed orders created today" loading={d.orders.loading} error={d.orders.error}
          lockedBy={d.canOrders ? undefined : PERMISSIONS.orderRead} onRetry={d.orders.reload} />
        <StatCard label="Pending orders" value={formatCount(d.orderStats.pending)}
          hint={`${formatCount(d.orderStats.inProgress)} more in progress`} loading={d.orders.loading}
          error={d.orders.error} lockedBy={d.canOrders ? undefined : PERMISSIONS.orderRead} onRetry={d.orders.reload} />
        <StatCard label="Products" value={products ? formatCount(products.length) : undefined}
          hint={products ? `${formatCount(products.filter((p) => p.isAvailable).length)} available` : undefined}
          loading={d.products.loading} error={d.products.error}
          lockedBy={d.canProducts ? undefined : PERMISSIONS.productRead} onRetry={d.reloadAll} />
        <StatCard label="Low-stock products" value={formatCount(lowStock)}
          hint="At or below reorder level" loading={d.inventory.loading} error={d.inventory.error}
          lockedBy={d.canInventory ? undefined : PERMISSIONS.inventoryRead} onRetry={d.reloadAll} />
        <StatCard label="Active branch" value={me.branchName ?? 'All branches'}
          hint={me.branchId != null ? 'Your access is limited to this branch'
            : d.branchCount != null ? `Organization-wide · ${d.branchCount} active branch${d.branchCount === 1 ? '' : 'es'}`
            : 'Organization-wide access'} />
      </div>

      <RecentOrders allowed={d.canOrders} loading={d.orders.loading} error={d.orders.error}
        orders={d.orderStats.recent} onRetry={d.orders.reload} />
    </>
  );
}
