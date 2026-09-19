using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> entity)
    {
        entity.HasIndex(e => e.RoleId, "IX_UserRoles_RoleID");

        entity.HasIndex(e => new { e.OrganizationUserId, e.RoleId },
            "UQ_UserRoles_OrganizationUser_Role").IsUnique();

        entity.Property(e => e.UserRoleId).HasColumnName("UserRoleID");
        entity.Property(e => e.OrganizationUserId).HasColumnName("OrganizationUserID");
        entity.Property(e => e.RoleId).HasColumnName("RoleID");
        entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
        entity.Property(e => e.AssignedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");

        entity.HasOne(d => d.OrganizationUser).WithMany(p => p.UserRoles)
            .HasPrincipalKey(p => new { p.OrganizationId, p.OrganizationUserId })
            .HasForeignKey(d => new { d.OrganizationId, d.OrganizationUserId })
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_UserRoles_OrganizationUsers_Tenant");

        entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
            .HasPrincipalKey(p => new { p.OrganizationId, p.RoleId })
            .HasForeignKey(d => new { d.OrganizationId, d.RoleId })
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_UserRoles_Roles_Tenant");
    }
}