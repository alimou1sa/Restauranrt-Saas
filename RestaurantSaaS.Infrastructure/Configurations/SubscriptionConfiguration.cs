using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{

    public void Configure(EntityTypeBuilder<Subscription> entity)
    {
        entity.HasIndex(e => e.OrganizationId, "IX_Subscriptions_OrganizationID");

        entity.HasIndex(e => e.Status, "IX_Subscriptions_Status");

        entity.Property(e => e.SubscriptionId).HasColumnName("SubscriptionID");
        entity.Property(e => e.BillingCycle)
            .HasMaxLength(20)
            .IsUnicode(false);
        entity.Property(e => e.CanceledAtUtc).HasPrecision(3);
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.CurrentPeriodEndUtc).HasPrecision(3);
        entity.Property(e => e.CurrentPeriodStartUtc).HasPrecision(3);
        entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
        entity.Property(e => e.PlanId).HasColumnName("PlanID");
        entity.Property(e => e.StartDateUtc).HasPrecision(3);
        entity.Property(e => e.Status)
            .HasMaxLength(30)
            .IsUnicode(false);
        entity.Property(e => e.UpdatedAtUtc).HasPrecision(3);

        entity.HasOne(d => d.Organization).WithMany(p => p.Subscriptions)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Subscriptions_Organizations");

        entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
            .HasForeignKey(d => d.PlanId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Subscriptions_Plans");
    }
}
