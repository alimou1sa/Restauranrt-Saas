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
    internal class SystemRoleConfiguration : IEntityTypeConfiguration<SystemRole>
    {
        public void Configure(EntityTypeBuilder<SystemRole> entity)
        {
            entity.ToTable("SystemRoles");

            entity.HasKey(e => e.SystemRoleId)
                .HasName("PK_SystemRoles");

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("UQ_SystemRoles_Name");

            entity.Property(e => e.SystemRoleId)
                .HasColumnName("SystemRoleID");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(300);
        }
    }
}
