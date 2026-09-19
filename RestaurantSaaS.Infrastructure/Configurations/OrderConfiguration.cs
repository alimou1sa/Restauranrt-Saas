using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{

    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.HasIndex(e => new { e.BranchId, e.CreatedAtUtc }, "IX_Orders_BranchID_CreatedAt");

        entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerID");

        entity.HasIndex(e => e.Status, "IX_Orders_Status");

        entity.HasIndex(e => new { e.BranchId, e.OrderId }, "UQ_Orders_Branch_Order").IsUnique();

        entity.HasIndex(e => new { e.BranchId, e.OrderNumber }, "UQ_Orders_Branch_OrderNumber").IsUnique();

        entity.Property(e => e.OrderId).HasColumnName("OrderID");
        entity.Property(e => e.BranchId).HasColumnName("BranchID");
        entity.Property(e => e.ClosedAtUtc).HasPrecision(3);
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
        entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.Notes).HasMaxLength(1000);
        entity.Property(e => e.OrderNumber).HasMaxLength(50);
        entity.Property(e => e.OrderType)
            .HasMaxLength(20)
            .IsUnicode(false);
        entity.Property(e => e.Status)
            .HasMaxLength(30)
            .IsUnicode(false);
        entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.TableId).HasColumnName("TableID");
        entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.UpdatedAtUtc).HasPrecision(3);

        entity.HasOne(d => d.Branch).WithMany(p => p.Orders)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Orders_Branches");

        entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
            .HasForeignKey(d => d.CustomerId)
            .HasConstraintName("FK_Orders_Customers");

        entity.HasOne(d => d.RestaurantTable).WithMany(p => p.Orders)
            .HasPrincipalKey(p => new { p.BranchId, p.TableId })
            .HasForeignKey(d => new { d.BranchId, d.TableId })
            .HasConstraintName("FK_Orders_Tables");
    }


}

