using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Category
{
    public int CategoryId { get; set; }

    public int MenuId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual Menu Menu { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
