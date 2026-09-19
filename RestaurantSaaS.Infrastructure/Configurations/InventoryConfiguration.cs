using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{

    public void Configure(EntityTypeBuilder<Inventory> entity)
    {
        entity.ToTable("Inventory");

        entity.HasIndex(e => e.BranchId, "IX_Inventory_BranchID");

        entity.HasIndex(e => e.ProductId, "IX_Inventory_ProductID");

        entity.HasIndex(e => new { e.BranchId, e.ProductId }, "UQ_Inventory_Branch_Product").IsUnique();

        entity.Property(e => e.InventoryId).HasColumnName("InventoryID");
        entity.Property(e => e.BranchId).HasColumnName("BranchID");
        entity.Property(e => e.ProductId).HasColumnName("ProductID");
        entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
        entity.Property(e => e.ReorderLevel).HasColumnType("decimal(18, 3)");
        entity.Property(e => e.UpdatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");

        entity.HasOne(d => d.Branch).WithMany(p => p.Inventories)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Inventory_Branches");

        entity.HasOne(d => d.Product).WithMany(p => p.Inventories)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Inventory_Products");
    }

}
