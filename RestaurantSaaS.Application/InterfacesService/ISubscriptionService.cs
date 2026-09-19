using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest;
using global::RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsResponse;

namespace RestaurantSaaS.Application.InterfacesService
{

    public interface ISubscriptionService
    {

        Task<SubscriptionResponse> CreateAsync(int organizationId, CreateSubscriptionRequest request);

        Task<SubscriptionResponse?> GetCurrentByOrganizationAsync(int organizationId);

        Task<SubscriptionResponse?> ChangePlanAsync(int subscriptionId, ChangeSubscriptionPlanRequest request);

        Task<SubscriptionResponse?> CancelAsync(int subscriptionId, CancelSubscriptionRequest request);
    }
}
