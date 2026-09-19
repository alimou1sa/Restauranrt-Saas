using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class MenuConfiguration : IEntityTypeConfiguration<Menu>
{

    public void Configure(EntityTypeBuilder<Menu> entity)
    {
        entity.HasIndex(e => e.BranchId, "IX_Menus_BranchID");

        entity.HasIndex(e => new { e.BranchId, e.MenuId }, "UQ_Menus_Branch_Menu").IsUnique();

        entity.HasIndex(e => new { e.BranchId, e.Name }, "UQ_Menus_Branch_Name").IsUnique();

        entity.Property(e => e.MenuId).HasColumnName("MenuID");
        entity.Property(e => e.BranchId).HasColumnName("BranchID");
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.Name).HasMaxLength(150);
        entity.Property(e => e.UpdatedAtUtc).HasPrecision(3);

        entity.HasOne(d => d.Branch).WithMany(p => p.Menus)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Menus_Branches");


    }
}