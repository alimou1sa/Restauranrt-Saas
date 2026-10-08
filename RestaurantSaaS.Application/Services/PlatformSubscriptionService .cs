using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest;
using RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsResponse;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Services
{
    public class PlatformSubscriptionService : IPlatformSubscriptionService
    {
        private readonly IAppDbContext _context;
        public PlatformSubscriptionService(IAppDbContext context) => _context = context;

        public async Task<List<SubscriptionResponse>> GetAllAsync()
            => await Projection().ToListAsync();

        public async Task<List<SubscriptionResponse>> GetByOrganizationAsync(int organizationId)
            => await Projection().Where(s => s.OrganizationId == organizationId).ToListAsync();

        public async Task<SubscriptionResponse?> ChangePlanAsync(int subscriptionId, ChangeSubscriptionPlanRequest request)
        {
            var subscription = await _context.Subscriptions.IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);
            if (subscription is null) return null;

            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.PlanId == request.PlanId);
            if (plan is null) throw new KeyNotFoundException($"Plan {request.PlanId} was not found.");

            subscription.PlanId = request.PlanId;
            subscription.BillingCycle = request.BillingCycle;
            subscription.CurrentPeriodEndUtc = CalculatePeriodEnd(subscription.CurrentPeriodStartUtc, request.BillingCycle);
            subscription.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ToResponse(subscription, plan.Name);
        }

        public async Task<SubscriptionResponse?> CancelAsync(int subscriptionId, CancelSubscriptionRequest request)
        {
            var subscription = await _context.Subscriptions.IgnoreQueryFilters()
                .Include(s => s.Plan).FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);
            if (subscription is null) return null;

            if (request.CancelImmediately) { subscription.Status = "Canceled"; subscription.CanceledAtUtc = DateTime.UtcNow; }
            else subscription.CancelAtPeriodEnd = true;

            subscription.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ToResponse(subscription, subscription.Plan.Name);
        }

        private IQueryable<SubscriptionResponse> Projection() => _context.Subscriptions.IgnoreQueryFilters().AsNoTracking()
            .Select(s => new SubscriptionResponse
            {
                SubscriptionId = s.SubscriptionId,
                OrganizationId = s.OrganizationId,
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
            });

        private static DateTime CalculatePeriodEnd(DateTime start, string cycle) => cycle switch
        {
            "Monthly" => start.AddMonths(1),
            "Yearly" => start.AddYears(1),
            _ => throw new InvalidOperationException($"'{cycle}' is not a valid billing cycle.")
        };

        private static SubscriptionResponse ToResponse(Subscription s, string planName) => new()
        {
            SubscriptionId = s.SubscriptionId,
            OrganizationId = s.OrganizationId,
            PlanId = s.PlanId,
            PlanName = planName,
            Status = s.Status,
            BillingCycle = s.BillingCycle,
            StartDateUtc = s.StartDateUtc,
            CurrentPeriodStartUtc = s.CurrentPeriodStartUtc,
            CurrentPeriodEndUtc = s.CurrentPeriodEndUtc,
            CancelAtPeriodEnd = s.CancelAtPeriodEnd,
            CanceledAtUtc = s.CanceledAtUtc,
            CreatedAtUtc = s.CreatedAtUtc,
            UpdatedAtUtc = s.UpdatedAtUtc
        };
    }
}
