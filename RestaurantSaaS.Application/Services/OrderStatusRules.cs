using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Services
{
    public static class OrderStatusRules
    {
        public const string Canceled = "Canceled";
        public const string Completed = "Completed";
        public const string Pending = "Pending";

        public static readonly string[] Flow = { "Pending", "Confirmed", "Preparing", "Ready", "Served", "Completed" };

        public static readonly string[] AllStatuses = Flow.Concat(new[] { Canceled }).ToArray();

         public const string ClosedOrderStatusMessage = "A Completed or Canceled order cannot change status.";
        public const string ClosedOrderModifyMessage = "A Completed or Canceled order cannot be modified.";
        public const string ClosedOrderItemsMessage = "Items cannot be changed on a Completed or Canceled order.";
        public const string ClosedOrderDeleteMessage = "A Completed or Canceled order cannot be deleted.";
        public const string BackwardStatusMessage = "Order status can only move forward: Pending, Confirmed, Preparing, Ready, Served, Completed. Use Canceled to cancel an open order.";
        public const string DeleteOnlyPendingMessage = "Only Pending orders can be deleted. Cancel the order instead.";
        public const string DeleteWithPaymentsMessage = "An order that has payments cannot be deleted. Cancel the order instead.";

        public const string ItemsHavePaymentsMessage = "Items cannot be changed on an order that has completed payments.";
        public const string DiscountHasPaymentsMessage = "The discount cannot be changed on an order that has completed payments.";

        public static string CompleteUnpaidMessage(decimal balanceDue) =>
            $"The order cannot be completed until its completed payments cover the total. Balance due: {balanceDue.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}.";

        public static bool IsClosed(string status) => status == Completed || status == Canceled;

         public static bool CanMove(string from, string to)
        {
            if (IsClosed(from)) return false;
            if (to == Canceled) return true;
            var fromIndex = Array.IndexOf(Flow, from);
            var toIndex = Array.IndexOf(Flow, to);
            return fromIndex >= 0 && toIndex > fromIndex;
        }
    }
}
