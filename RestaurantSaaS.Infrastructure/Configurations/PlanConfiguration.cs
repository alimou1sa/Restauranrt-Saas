using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class PlanConfiguration : IEntityTypeConfiguration<Plan>
{

    public void Configure(EntityTypeBuilder<Plan> entity)
    {
        entity.HasIndex(e => e.Name, "UQ_Plans_Name").IsUnique();

        entity.Property(e => e.PlanId).HasColumnName("PlanID");
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.Name).HasMaxLength(100);
        entity.Property(e => e.YearlyPrice).HasColumnType("decimal(18, 2)");
    }

}
