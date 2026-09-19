using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{

    public void Configure(EntityTypeBuilder<OrderItem> entity)
    {
        entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderID");

        entity.HasIndex(e => e.ProductId, "IX_OrderItems_ProductID");

        entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
        entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.LineTotal)
            .HasComputedColumnSql("([UnitPrice]*[Quantity]-[DiscountAmount])", true)
            .HasColumnType("decimal(38, 5)");
        entity.Property(e => e.OrderId).HasColumnName("OrderID");
        entity.Property(e => e.ProductId).HasColumnName("ProductID");
        entity.Property(e => e.ProductName).HasMaxLength(150);
        entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
        entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

        entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_OrderItems_Orders");

        entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_OrderItems_Products");
    }

}
