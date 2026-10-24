using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{

    public void Configure(EntityTypeBuilder<Payment> entity)
    {
        entity.HasIndex(e => e.OrderId, "IX_Payments_OrderID");

        entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
        entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.OrderId).HasColumnName("OrderID");
        entity.Property(e => e.PaidAtUtc).HasPrecision(3);
        entity.Property(e => e.PaymentMethod)
            .HasMaxLength(30)
            .IsUnicode(false);
        entity.Property(e => e.Status)
            .HasMaxLength(30)
            .IsUnicode(false);
        entity.Property(e => e.TransactionReference).HasMaxLength(200);

        entity.HasOne(d => d.Order).WithMany(p => p.Payments)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Payments_Orders");

        entity.Property(e => e.CreatedByOrganizationUserId).HasColumnName("CreatedByOrganizationUserID");
        entity.Property(e => e.ConfirmedByOrganizationUserId).HasColumnName("ConfirmedByOrganizationUserID");

        // No navigation properties: only the ids are kept. NoAction so removing a member never cascades into payments.
        entity.HasOne<OrganizationUser>().WithMany()
            .HasForeignKey(d => d.CreatedByOrganizationUserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Payments_CreatedBy_OrganizationUsers");

        entity.HasOne<OrganizationUser>().WithMany()
            .HasForeignKey(d => d.ConfirmedByOrganizationUserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Payments_ConfirmedBy_OrganizationUsers");
    }


}
