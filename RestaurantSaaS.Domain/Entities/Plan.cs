using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Plan
{
    public int PlanId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal MonthlyPrice { get; set; }

    public decimal YearlyPrice { get; set; }

    public int? MaxBranches { get; set; }

    public int? MaxUsers { get; set; }

    public int? MaxProducts { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
