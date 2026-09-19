using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> entity)
    {
        entity.HasIndex(e => e.OrganizationId, "IX_Branches_OrganizationID");
        entity.HasIndex(e => new { e.OrganizationId, e.BranchId }, "UQ_Branches_Organization_Branch").IsUnique();
        entity.HasIndex(e => new { e.OrganizationId, e.Name }, "UQ_Branches_Organization_Name").IsUnique();

        entity.Property(e => e.BranchId).HasColumnName("BranchID");
        entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
        entity.Property(e => e.Name).HasMaxLength(150);
        entity.Property(e => e.Address).HasMaxLength(300);
        entity.Property(e => e.Phone).HasMaxLength(30);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.CreatedAtUtc).HasPrecision(3).HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.UpdatedAtUtc).HasPrecision(3);

        entity.HasOne(d => d.Organization).WithMany(p => p.Branches)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Branches_Organizations");
    }
}
