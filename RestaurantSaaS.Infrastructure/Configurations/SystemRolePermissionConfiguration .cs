using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Infrastructure.Configurations
{
    internal class SystemRolePermissionConfiguration : IEntityTypeConfiguration<SystemRolePermission>
    {
        public void Configure(EntityTypeBuilder<SystemRolePermission> entity)
        {
            entity.HasIndex(e => e.PermissionId, "IX_SystemRolePermissions_PermissionID");

            entity.HasIndex(e => new { e.SystemRoleId, e.PermissionId }, "UQ_SystemRolePermissions_Role_Permission").IsUnique();

            entity.Property(e => e.SystemRolePermissionId).HasColumnName("SystemRolePermissionID");
            entity.Property(e => e.SystemRoleId).HasColumnName("SystemRoleID");
            entity.Property(e => e.PermissionId).HasColumnName("PermissionID");

            entity.HasOne(d => d.SystemRole).WithMany(p => p.SystemRolePermissions)
                .HasForeignKey(d => d.SystemRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SystemRolePermissions_SystemRoles");

            entity.HasOne(d => d.Permission).WithMany(p => p.SystemRolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SystemRolePermissions_Permissions");
        }
    }
}
