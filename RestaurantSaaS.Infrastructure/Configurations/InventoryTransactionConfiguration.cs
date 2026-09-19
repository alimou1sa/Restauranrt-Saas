using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{

    public void Configure(EntityTypeBuilder<InventoryTransaction> entity)
    {
        entity.HasIndex(e => new { e.InventoryId, e.CreatedAtUtc }, "IX_InventoryTransactions_InventoryID_CreatedAt");

        entity.Property(e => e.InventoryTransactionId).HasColumnName("InventoryTransactionID");
        entity.Property(e => e.CreatedAtUtc)
            .HasPrecision(3)
            .HasDefaultValueSql("(sysutcdatetime())");
        entity.Property(e => e.InventoryId).HasColumnName("InventoryID");
        entity.Property(e => e.Notes).HasMaxLength(500);
        entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
        entity.Property(e => e.ReferenceId).HasColumnName("ReferenceID");
        entity.Property(e => e.ReferenceType)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.TransactionType)
            .HasMaxLength(30)
            .IsUnicode(false);

        entity.HasOne(d => d.Inventory).WithMany(p => p.InventoryTransactions)
            .HasForeignKey(d => d.InventoryId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_InventoryTransactions_Inventory");
    }
}