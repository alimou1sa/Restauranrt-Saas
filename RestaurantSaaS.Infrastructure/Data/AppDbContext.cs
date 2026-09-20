using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RestaurantSaaS.Application.Common;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RestaurantSaaS.Infrastructure.Data;

public partial class AppDbContext : DbContext, IAppDbContext
{
    private readonly ICurrentUser _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser): base(options)
    {
        _currentUser = currentUser;
    }


    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<OrganizationUser> OrganizationUsers { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Plan> Plans { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<RestaurantTable> RestaurantTables { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<SystemRole> SystemRoles { get; set; }

  /*  protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }*/
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await Database.BeginTransactionAsync();
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);


        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
                modelBuilder.Entity(clrType).HasQueryFilter(BuildTenantFilter(clrType));
        }


        ApplyBranchScopedFilter<Menu>(modelBuilder, e => e.Branch);
        ApplyBranchScopedFilter<RestaurantTable>(modelBuilder, e => e.Branch);
        ApplyBranchScopedFilter<Inventory>(modelBuilder, e => e.Branch);
        ApplyBranchScopedFilter<Order>(modelBuilder, e => e.Branch);
        ApplyBranchScopedFilter<Category>(modelBuilder, e => e.Menu.Branch);
        ApplyBranchScopedFilter<Product>(modelBuilder, e => e.Category.Menu.Branch);
        ApplyBranchScopedFilter<OrderItem>(modelBuilder, e => e.Order.Branch);
        ApplyBranchScopedFilter<Payment>(modelBuilder, e => e.Order.Branch);
        ApplyBranchScopedFilter<InventoryTransaction>(modelBuilder, e => e.Inventory.Branch);

        OnModelCreatingPartial(modelBuilder);
    }

    private LambdaExpression BuildTenantFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");
        var property = Expression.Property(parameter, nameof(ITenantEntity.OrganizationId));

        var currentUserConst = Expression.Constant(_currentUser);
        var orgIdProp = Expression.Property(currentUserConst, nameof(ICurrentUser.OrganizationId));
        var hasValue = Expression.Property(orgIdProp, nameof(Nullable<int>.HasValue));
        var value = Expression.Property(orgIdProp, nameof(Nullable<int>.Value));

        var body = Expression.AndAlso(hasValue, Expression.Equal(property, value));
        return Expression.Lambda(body, parameter);
    }

    private void ApplyBranchScopedFilter<TEntity>(ModelBuilder modelBuilder
        ,Expression<Func<TEntity, Branch>> branchSelector) where TEntity : class
    {
        var parameter = branchSelector.Parameters[0];
        var branchAccess = branchSelector.Body;

        var currentUserConst = Expression.Constant(_currentUser);

        var branchOrgId = Expression.Property(branchAccess, nameof(Branch.OrganizationId));
        var orgIdProp = Expression.Property(currentUserConst, nameof(ICurrentUser.OrganizationId));
        var orgHasValue = Expression.Property(orgIdProp, nameof(Nullable<int>.HasValue));
        var orgValue = Expression.Property(orgIdProp, nameof(Nullable<int>.Value));
        var tenantCheck = Expression.AndAlso(orgHasValue, Expression.Equal(branchOrgId, orgValue));

        var branchIdOfChain = Expression.Property(branchAccess, nameof(Branch.BranchId));
        var branchIdProp = Expression.Property(currentUserConst, nameof(ICurrentUser.BranchId));
        var branchHasValue = Expression.Property(branchIdProp, nameof(Nullable<int>.HasValue));
        var branchNotRestricted = Expression.Not(branchHasValue);
        var branchValue = Expression.Property(branchIdProp, nameof(Nullable<int>.Value));
        var branchCheck = Expression.OrElse(branchNotRestricted, Expression.Equal(branchIdOfChain, branchValue));

        var body = Expression.AndAlso(tenantCheck, branchCheck);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        modelBuilder.Entity<TEntity>().HasQueryFilter(lambda);
    }

}
