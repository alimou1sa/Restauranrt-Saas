using RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest;
using RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{
    public interface IPlatformSubscriptionService
    {
        Task<List<SubscriptionResponse>> GetAllAsync();
        Task<List<SubscriptionResponse>> GetByOrganizationAsync(int organizationId);
        Task<SubscriptionResponse?> ChangePlanAsync(int subscriptionId, ChangeSubscriptionPlanRequest request);
        Task<SubscriptionResponse?> CancelAsync(int subscriptionId, CancelSubscriptionRequest request);
    }
}
