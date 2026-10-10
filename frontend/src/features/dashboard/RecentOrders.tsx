import { EmptyState, ErrorState, Spinner } from '../../components/Feedback';
import { Icon } from '../../components/Icon';
import type { OrderListResponse } from '../../types/api';
import { formatAmount, formatDateTime } from '../../utils/format';

interface Props {
  allowed: boolean;
  loading: boolean;
  error: unknown;
  orders: OrderListResponse[];
  onRetry: () => void;
}

export function RecentOrders({ allowed, loading, error, orders, onRetry }: Props) {
  return (
    <section className="card recent-orders" aria-labelledby="recent-orders-title">
      <div className="recent-orders-heading">
        <div>
          <h2 id="recent-orders-title" className="section-title">Recent orders</h2>
          <p className="muted small">The latest activity across your accessible branch or organization.</p>
        </div>
        <span className="orders-count">{allowed ? orders.length : '—'} <span>shown</span></span>
      </div>
      {!allowed ? (
        <div className="permission-note"><Icon name="roles" size={17} /><p>Order information is hidden because your role does not include <code>order.read</code>.</p></div>
      ) : loading ? (
        <div className="state"><Spinner label="Loading orders" /></div>
      ) : error ? (
        <ErrorState error={error} onRetry={onRetry} />
      ) : orders.length === 0 ? (
        <EmptyState title="No orders yet" description="New orders will appear here as soon as they are created." />
      ) : (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th scope="col">Order</th>
                <th scope="col">Branch</th>
                <th scope="col">Type</th>
                <th scope="col">Status</th>
                <th scope="col" className="num">Total</th>
                <th scope="col">Created</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o) => (
                <tr key={o.orderId}>
                  <td><strong>{o.orderNumber}</strong>{o.tableName && <span className="muted"> · {o.tableName}</span>}</td>
                  <td>{o.branchName}</td>
                  <td>{o.orderType}</td>
                  <td><span className={`badge badge-${o.status.toLowerCase()}`}>{o.status}</span></td>
                  <td className="num">{formatAmount(o.totalAmount)}</td>
                  <td>{formatDateTime(o.createdAtUtc)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
