using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{

    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.HasIndex(e => e.PermissionId, "IX_RolePermissions_PermissionID");

        entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "UQ_RolePermissions_Role_Permission").IsUnique();

        entity.Property(e => e.RolePermissionId).HasColumnName("RolePermissionID");
        entity.Property(e => e.PermissionId).HasColumnName("PermissionID");
        entity.Property(e => e.RoleId).HasColumnName("RoleID");

        entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
            .HasForeignKey(d => d.PermissionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_RolePermissions_Permissions");

        entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_RolePermissions_Roles");
    }
}
