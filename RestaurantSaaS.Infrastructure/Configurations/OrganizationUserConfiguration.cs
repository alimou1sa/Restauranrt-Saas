using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class OrganizationUserConfiguration : IEntityTypeConfiguration<OrganizationUser>
{

    public void Configure(EntityTypeBuilder<OrganizationUser> entity)
    {
        entity.HasIndex(e => e.BranchId, "IX_OrganizationUsers_BranchID");

        entity.HasIndex(e => e.UserId, "IX_OrganizationUsers_UserID");

        entity.HasIndex(e => new { e.OrganizationId, e.UserId }, "UQ_OrganizationUsers_Organization_User").IsUnique();

        entity.Property(e => e.OrganizationUserId).HasColumnName("OrganizationUserID");
        entity.Property(e => e.BranchId).HasColumnName("BranchID");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.JoinedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
        entity.Property(e => e.RemovedAtUtc).HasPrecision(3);
        entity.Property(e => e.UserId).HasColumnName("UserID");

        entity.HasOne(d => d.Organization).WithMany(p => p.OrganizationUsers)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_OrganizationUsers_Organizations");

        entity.HasOne(d => d.User).WithMany(p => p.OrganizationUsers)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_OrganizationUsers_Users");

        entity.HasOne(d => d.Branch).WithMany(p => p.OrganizationUsers)
            .HasPrincipalKey(p => new { p.OrganizationId, p.BranchId })
            .HasForeignKey(d => new { d.OrganizationId, d.BranchId })
            .HasConstraintName("FK_OrganizationUsers_Branch");
    }

}