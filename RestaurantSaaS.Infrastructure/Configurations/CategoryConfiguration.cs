using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {

    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.HasIndex(e => e.MenuId, "IX_Categories_MenuID");

        entity.HasIndex(e => new { e.MenuId, e.CategoryId }, "UQ_Categories_Menu_Category").IsUnique();

        entity.HasIndex(e => new { e.MenuId, e.Name }, "UQ_Categories_Menu_Name").IsUnique();

        entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.MenuId).HasColumnName("MenuID");
        entity.Property(e => e.Name).HasMaxLength(100);

        entity.HasOne(d => d.Menu).WithMany(p => p.Categories)
            .HasForeignKey(d => d.MenuId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Categories_Menus");

    }



}

