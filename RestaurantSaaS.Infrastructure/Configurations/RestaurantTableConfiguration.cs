using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{

    public void Configure(EntityTypeBuilder<RestaurantTable> entity)
    {

        entity.HasKey(e => e.TableId);

        entity.HasIndex(e => e.BranchId, "IX_RestaurantTables_BranchID");

        entity.HasIndex(e => new { e.BranchId, e.TableNumber }, "UQ_RestaurantTables_Branch_Number").IsUnique();

        entity.HasIndex(e => new { e.BranchId, e.TableId }, "UQ_RestaurantTables_Branch_Table").IsUnique();

        entity.HasIndex(e => e.QRCodeToken, "UQ_RestaurantTables_QRCodeToken").IsUnique();

        entity.Property(e => e.TableId).HasColumnName("TableID");
        entity.Property(e => e.BranchId).HasColumnName("BranchID");
        entity.Property(e => e.Capacity).HasDefaultValue(2);
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.QRCodeToken)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("QRCodeToken");
        entity.Property(e => e.TableNumber).HasMaxLength(30);

        entity.HasOne(d => d.Branch).WithMany(p => p.RestaurantTables)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_RestaurantTables_Branches");

    }
}
