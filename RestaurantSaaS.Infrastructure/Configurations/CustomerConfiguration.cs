using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{

    public void Configure(EntityTypeBuilder<Customer> entity)
    {
        entity.HasIndex(e => e.OrganizationId, "IX_Customers_OrganizationID");

        entity.HasIndex(e => new { e.OrganizationId, e.CustomerId }, "UQ_Customers_Organization_Customer").IsUnique();

        entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Email).HasMaxLength(320);
        entity.Property(e => e.FirstName).HasMaxLength(100);
        entity.Property(e => e.LastName).HasMaxLength(100);
        entity.Property(e => e.Notes).HasMaxLength(1000);
        entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");
        entity.Property(e => e.Phone).HasMaxLength(30);
        entity.Property(e => e.UpdatedAtUtc).HasPrecision(3);

        entity.HasOne(d => d.Organization).WithMany(p => p.Customers)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Customers_Organizations");
    }


}
