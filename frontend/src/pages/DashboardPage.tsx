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
  const firstName = me.firstName?.trim() || 'there';
  const today = new Intl.DateTimeFormat(undefined, { weekday: 'long', month: 'long', day: 'numeric' }).format(new Date());

  return (
    <>
      <section className="dashboard-welcome">
        <div className="welcome-copy">
          <p className="eyebrow"><span className="eyebrow-dot" /> RESTAURANT OVERVIEW</p>
          <h1 className="welcome-title">Good to see you, {firstName}.</h1>
          <p className="welcome-subtitle">Here’s what’s happening with your restaurant today.</p>
          <div className="welcome-meta">
            <span><Icon name="building" size={15} /> {me.organizationName}</span>
            <span className="meta-separator" aria-hidden="true">/</span>
            <span>{me.branchName ?? 'All branches'}</span>
          </div>
        </div>
        <div className="welcome-actions">
          <span className="today-label">{today}</span>
          <button type="button" className="btn btn-welcome"
            onClick={() => { d.reloadAll(); toast.info('Refreshing dashboard…'); }}>
            <Icon name="refresh" size={16} /> Refresh data
          </button>
        </div>
        <div className="welcome-decoration" aria-hidden="true"><span /><span /><span /></div>
      </section>

      <section className="dashboard-section" aria-labelledby="overview-title">
        <div className="section-heading">
          <div>
            <h2 id="overview-title" className="section-title">At a glance</h2>
            <p className="muted small">A live snapshot based on the access available to your role.</p>
          </div>
          <span className="live-indicator"><span /> Current overview</span>
        </div>

        <div className="grid-stats">
          <StatCard icon="orders" tone="orange" label="Today's orders"
            value={d.canOrders ? formatCount(d.orderStats.todayOrders) : undefined}
            hint="Created today, excluding canceled" loading={d.orders.loading} error={d.orders.error}
            lockedBy={d.canOrders ? undefined : PERMISSIONS.orderRead} onRetry={d.orders.reload} />
          <StatCard icon="products" tone="blue" label="Today's revenue"
            value={formatAmount(d.orderStats.todayRevenue)}
            hint="Completed orders created today" loading={d.orders.loading} error={d.orders.error}
            lockedBy={d.canOrders ? undefined : PERMISSIONS.orderRead} onRetry={d.orders.reload} />
          <StatCard icon="orders" tone="violet" label="Pending orders"
            value={formatCount(d.orderStats.pending)}
            hint={`${formatCount(d.orderStats.inProgress)} more in progress`} loading={d.orders.loading}
            error={d.orders.error} lockedBy={d.canOrders ? undefined : PERMISSIONS.orderRead} onRetry={d.orders.reload} />
          <StatCard icon="menu" tone="green" label="Products"
            value={products ? formatCount(products.length) : undefined}
            hint={products ? `${formatCount(products.filter((p) => p.isAvailable).length)} available` : undefined}
            loading={d.products.loading} error={d.products.error}
            lockedBy={d.canProducts ? undefined : PERMISSIONS.productRead} onRetry={d.reloadAll} />
          <StatCard icon="inventory" tone="orange" label="Low-stock products"
            value={formatCount(lowStock)} hint="At or below reorder level"
            loading={d.inventory.loading} error={d.inventory.error}
            lockedBy={d.canInventory ? undefined : PERMISSIONS.inventoryRead} onRetry={d.reloadAll} />
          <StatCard icon="branches" tone="blue" label="Active branch"
            value={me.branchName ?? 'All branches'}
            hint={me.branchId != null ? 'Your access is limited to this branch'
              : d.branchCount != null ? `Organization-wide · ${d.branchCount} active branch${d.branchCount === 1 ? '' : 'es'}`
              : 'Organization-wide access'} />
        </div>
      </section>

      <RecentOrders allowed={d.canOrders} loading={d.orders.loading} error={d.orders.error}
        orders={d.orderStats.recent} onRetry={d.orders.reload} />
    </>
  );
}
