using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{

    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

        entity.Property(e => e.UserId).HasColumnName("UserID");
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.Email).HasMaxLength(320);
        entity.Property(e => e.FirstName).HasMaxLength(100);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.LastLoginAtUtc).HasPrecision(3);
        entity.Property(e => e.LastName).HasMaxLength(100);
        entity.Property(e => e.PasswordHash).HasMaxLength(500);
        entity.Property(e => e.Phone).HasMaxLength(30);
        entity.Property(e => e.UpdatedAtUtc).HasPrecision(3);
    }
}
