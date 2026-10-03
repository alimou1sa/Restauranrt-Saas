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
    internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> entity)
        {
            entity.HasIndex(e => e.TokenHash, "UQ_RefreshTokens_TokenHash").IsUnique();
            entity.HasIndex(e => e.OrganizationUserId, "IX_RefreshTokens_OrganizationUserID");

            entity.Property(e => e.RefreshTokenId).HasColumnName("RefreshTokenID");
            entity.Property(e => e.OrganizationUserId).HasColumnName("OrganizationUserID");
            entity.Property(e => e.TokenHash).HasMaxLength(256);
            entity.Property(e => e.ExpiresAtUtc).HasPrecision(3);
            entity.Property(e => e.RevokedAtUtc).HasPrecision(3);
            entity.Property(e => e.ReplacedByTokenId).HasColumnName("ReplacedByTokenID");
            entity.Property(e => e.CreatedAtUtc).HasPrecision(3).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.OrganizationUser).WithMany()
                .HasForeignKey(d => d.OrganizationUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_OrganizationUsers");

            entity.HasOne<RefreshToken>().WithMany()
                .HasForeignKey(e => e.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RefreshTokens_ReplacedBy");
        }
    }
}
