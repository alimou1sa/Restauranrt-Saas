using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{

    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.HasIndex(e => e.Code, "UQ_Permissions_Code").IsUnique();

        entity.Property(e => e.PermissionId).HasColumnName("PermissionID");
        entity.Property(e => e.Code).HasMaxLength(100);
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.Name).HasMaxLength(150);
    }

}
