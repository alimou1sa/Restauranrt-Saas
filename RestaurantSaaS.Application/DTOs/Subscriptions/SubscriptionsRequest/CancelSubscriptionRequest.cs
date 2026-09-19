using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest
{

    public class CancelSubscriptionRequest
    {
        public bool CancelImmediately { get; set; }
    }
}
