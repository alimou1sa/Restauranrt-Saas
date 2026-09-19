using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{

    public void Configure(EntityTypeBuilder<Organization> entity)
    {
        entity.HasIndex(e => e.Slug, "UQ_Organizations_Slug").IsUnique();

        entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Email).HasMaxLength(320);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.Name).HasMaxLength(150);
        entity.Property(e => e.Phone).HasMaxLength(30);
        entity.Property(e => e.Slug).HasMaxLength(100);
        entity.Property(e => e.UpdatedAtUtc).HasPrecision(3);
    }
}