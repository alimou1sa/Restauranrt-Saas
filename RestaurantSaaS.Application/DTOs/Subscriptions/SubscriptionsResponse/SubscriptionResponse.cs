using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsResponse
{
    // PlanName is flattened from the related Plan for convenient display.
    public class SubscriptionResponse
    {
        public int SubscriptionId { get; set; }

        public int OrganizationId { get; set; }

        public int PlanId { get; set; }

        public string PlanName { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string BillingCycle { get; set; } = null!;

        public DateTime StartDateUtc { get; set; }

        public DateTime CurrentPeriodStartUtc { get; set; }

        public DateTime CurrentPeriodEndUtc { get; set; }

        public bool CancelAtPeriodEnd { get; set; }

        public DateTime? CanceledAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
    }
}
