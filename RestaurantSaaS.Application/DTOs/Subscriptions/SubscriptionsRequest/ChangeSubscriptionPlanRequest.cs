using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest
{



    public class ChangeSubscriptionPlanRequest
    {
        [Required]
        public int PlanId { get; set; }

        [Required, MaxLength(20)]
        public string BillingCycle { get; set; } = null!;
    }
}
