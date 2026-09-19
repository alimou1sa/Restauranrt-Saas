using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest
{

    // OrganizationId comes from the route/tenant context. Status and all
    // period dates (StartDateUtc/CurrentPeriodStartUtc/CurrentPeriodEndUtc)
    // are computed server-side (billing logic / payment provider), never
    // supplied by the client.
    public class CreateSubscriptionRequest
    {
        [Required]
        public int PlanId { get; set; }

        [Required, MaxLength(20)]
        public string BillingCycle { get; set; } = null!;
    }
}
