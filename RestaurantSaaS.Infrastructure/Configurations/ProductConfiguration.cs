using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{

    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryID");

        entity.HasIndex(e => new { e.CategoryId, e.Name }, "UQ_Products_Category_Name").IsUnique();

        entity.HasIndex(e => new { e.CategoryId, e.ProductId }, "UQ_Products_Category_Product").IsUnique();

        entity.Property(e => e.ProductId).HasColumnName("ProductID");
        entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Description).HasMaxLength(1000);
        entity.Property(e => e.ImageUrl).HasMaxLength(1000);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.IsAvailable).HasDefaultValue(true);
        entity.Property(e => e.Name).HasMaxLength(150);
        entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.UpdatedAtUtc).HasPrecision(3);

        entity.HasOne(d => d.Category).WithMany(p => p.Products)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Products_Categories");
    }

}
