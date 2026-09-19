using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest;
    using global::RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaaS.Application.Services
{


    public class SubscriptionService : ISubscriptionService
    {
        private readonly IAppDbContext _context;

        public SubscriptionService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionResponse> CreateAsync(int organizationId, CreateSubscriptionRequest request)
        {
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.OrganizationId == organizationId);

            if (!organizationExists)
                throw new KeyNotFoundException($"Organization {organizationId} was not found.");

            var plan = await _context.Plans
                .FirstOrDefaultAsync(p => p.PlanId == request.PlanId);

            if (plan is null)
                throw new KeyNotFoundException($"Plan {request.PlanId} was not found.");

            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.OrganizationId == organizationId && s.Status == "Active");

            if (hasActiveSubscription)
                throw new InvalidOperationException("This organization already has an active subscription.");

            var now = DateTime.UtcNow;
            var periodEnd = CalculatePeriodEnd(now, request.BillingCycle);

            var subscription = new Subscription
            {
                OrganizationId = organizationId,
                PlanId = request.PlanId,
                Status = "Active",
                BillingCycle = request.BillingCycle,
                StartDateUtc = now,
                CurrentPeriodStartUtc = now,
                CurrentPeriodEndUtc = periodEnd,
                CancelAtPeriodEnd = false,
                CreatedAtUtc = now
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return ToResponse(subscription, plan.Name);
        }

        public async Task<SubscriptionResponse?> GetCurrentByOrganizationAsync(int organizationId)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.OrganizationId == organizationId)
                .OrderByDescending(s => s.CreatedAtUtc)
                .Select(s => new SubscriptionResponse
                {
                    SubscriptionId = s.SubscriptionId,
                    PlanId = s.PlanId,
                    PlanName = s.Plan.Name,
                    Status = s.Status,
                    BillingCycle = s.BillingCycle,
                    StartDateUtc = s.StartDateUtc,
                    CurrentPeriodStartUtc = s.CurrentPeriodStartUtc,
                    CurrentPeriodEndUtc = s.CurrentPeriodEndUtc,
                    CancelAtPeriodEnd = s.CancelAtPeriodEnd,
                    CanceledAtUtc = s.CanceledAtUtc,
                    CreatedAtUtc = s.CreatedAtUtc,
                    UpdatedAtUtc = s.UpdatedAtUtc
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SubscriptionResponse?> ChangePlanAsync(int subscriptionId, ChangeSubscriptionPlanRequest request)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription is null)
                return null;

            var plan = await _context.Plans
                .FirstOrDefaultAsync(p => p.PlanId == request.PlanId);

            if (plan is null)
                throw new KeyNotFoundException($"Plan {request.PlanId} was not found.");

            subscription.PlanId = request.PlanId;
            subscription.BillingCycle = request.BillingCycle;
            subscription.CurrentPeriodEndUtc = CalculatePeriodEnd(subscription.CurrentPeriodStartUtc, request.BillingCycle);
            subscription.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToResponse(subscription, plan.Name);
        }

        public async Task<SubscriptionResponse?> CancelAsync(int subscriptionId, CancelSubscriptionRequest request)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

            if (subscription is null)
                return null;

            if (request.CancelImmediately)
            {
                subscription.Status = "Canceled";
                subscription.CanceledAtUtc = DateTime.UtcNow;
            }
            else
            {
                subscription.CancelAtPeriodEnd = true;
            }

            subscription.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToResponse(subscription, subscription.Plan.Name);
        }

        private static DateTime CalculatePeriodEnd(DateTime periodStart, string billingCycle)
        {
            return billingCycle switch
            {
                "Monthly" => periodStart.AddMonths(1),
                "Yearly" => periodStart.AddYears(1),
                _ => throw new InvalidOperationException($"'{billingCycle}' is not a valid billing cycle.")
            };
        }

        private static SubscriptionResponse ToResponse(Subscription subscription, string planName)
        {
            return new SubscriptionResponse
            {
                SubscriptionId = subscription.SubscriptionId,
                PlanId = subscription.PlanId,
                PlanName = planName,
                Status = subscription.Status,
                BillingCycle = subscription.BillingCycle,
                StartDateUtc = subscription.StartDateUtc,
                CurrentPeriodStartUtc = subscription.CurrentPeriodStartUtc,
                CurrentPeriodEndUtc = subscription.CurrentPeriodEndUtc,
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                CanceledAtUtc = subscription.CanceledAtUtc,
                CreatedAtUtc = subscription.CreatedAtUtc,
                UpdatedAtUtc = subscription.UpdatedAtUtc
            };
        }
    }
}
