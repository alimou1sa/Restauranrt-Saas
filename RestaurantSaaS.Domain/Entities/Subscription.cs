using RestaurantSaaS.Application.Common;
using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Subscription: ITenantEntity
{
    public int SubscriptionId { get; set; }

    public int OrganizationId { get; set; }

    public int PlanId { get; set; }

    public string Status { get; set; } = null!;

    public string BillingCycle { get; set; } = null!;

    public DateTime StartDateUtc { get; set; }

    public DateTime CurrentPeriodStartUtc { get; set; }

    public DateTime CurrentPeriodEndUtc { get; set; }

    public bool CancelAtPeriodEnd { get; set; }

    public DateTime? CanceledAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual Plan Plan { get; set; } = null!;
}
