using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("Roles");

        // Primary Key
        entity.HasKey(e => e.RoleId)
            .HasName("PK_Roles");

        // Indexes
        entity.HasIndex(e => e.OrganizationId)
            .HasDatabaseName("IX_Roles_OrganizationID");

        entity.HasIndex(e => e.SystemRoleId)
            .HasDatabaseName("IX_Roles_SystemRoleID");

        // Custom Role unique name per Organization
        entity.HasIndex(e => new { e.OrganizationId, e.Name })
            .HasDatabaseName("UX_Roles_Organization_CustomName")
            .IsUnique()
            .HasFilter("[SystemRoleID] IS NULL AND [Name] IS NOT NULL");

        // Only one instance of each SystemRole per Organization
        entity.HasIndex(e => new { e.OrganizationId, e.SystemRoleId })
            .HasDatabaseName("UX_Roles_Organization_SystemRole")
            .IsUnique()
            .HasFilter("[SystemRoleID] IS NOT NULL");

        // Properties
        entity.Property(e => e.RoleId)
            .HasColumnName("RoleID");

        entity.Property(e => e.OrganizationId)
            .HasColumnName("OrganizationID");

        entity.Property(e => e.SystemRoleId)
            .HasColumnName("SystemRoleID");

        entity.Property(e => e.Name)
            .HasMaxLength(100);

        entity.Property(e => e.Description)
            .HasMaxLength(300);

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);

        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");

        // Organization relationship
        entity.HasOne(e => e.Organization)
            .WithMany(e => e.Roles)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Roles_Organizations");

        // SystemRole relationship
        entity.HasOne(e => e.SystemRole)
            .WithMany(e => e.Roles)
            .HasForeignKey(e => e.SystemRoleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Roles_SystemRoles");
    }
}