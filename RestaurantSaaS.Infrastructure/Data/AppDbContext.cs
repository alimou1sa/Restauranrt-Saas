using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Auth;
using System.Linq.Expressions;


namespace RestaurantSaaS.Infrastructure.Data;

public partial class AppDbContext : DbContext, IAppDbContext
{
    private readonly ICurrentTenant _currentTenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenant currentTenant) : base(options)
    {
        _currentTenant = currentTenant;
    }

    // =========================================================
    // Current Tenant Context
    // =========================================================

    public int? CurrentOrganizationId => _currentTenant.OrganizationId;

    public int? CurrentBranchId => _currentTenant.BranchId;

    //-----------------------------------------------------------

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
    public virtual DbSet<SystemRolePermission> SystemRolePermissions { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    // =========================================================
    // Model Configuration
    // =========================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ApplyTenantFilters(modelBuilder);
        ApplyBranchScopedFilters(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }


    // =========================================================
    // Organization / Tenant Filters
    // =========================================================

    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(IBranchOwnedDirect).IsAssignableFrom(clrType))
                continue;

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasQueryFilter(BuildTenantFilter(clrType));
            }
        }
    }


    private LambdaExpression BuildTenantFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");

        var organizationIdProperty = Expression.Property(parameter, nameof(ITenantEntity.OrganizationId));

        var contextExpression = Expression.Constant(this);

        var currentOrganizationId = Expression.Property(contextExpression, nameof(CurrentOrganizationId));

        // Lifted comparison (int? == int?) so a missing tenant becomes "orgId = NULL"
        // (i.e. no rows) instead of touching Nullable.Value. EF evaluates the right-hand
        // side eagerly as a query parameter, so reading .Value threw
        // "Nullable object must have a value" whenever there was no organization context.
        var organizationMatches = Expression.Equal(
            Expression.Convert(organizationIdProperty, typeof(int?)),
            currentOrganizationId);

        return Expression.Lambda(organizationMatches, parameter);
    }


    // =========================================================
    // Branch Scoped Filters
    // =========================================================

    private void ApplyBranchScopedFilters(ModelBuilder modelBuilder)
    {

        ApplyBranchScopedFilter<Menu>(modelBuilder, e => e.Branch);

        ApplyBranchScopedFilter<RestaurantTable>(modelBuilder, e => e.Branch);

        ApplyBranchScopedFilter<Inventory>(modelBuilder, e => e.Branch);


        ApplyDirectTenantAndBranchFilter<Order>(modelBuilder);


        ApplyBranchScopedFilter<Category>(modelBuilder, e => e.Menu.Branch);

        ApplyBranchScopedFilter<Product>(modelBuilder, e => e.Category.Menu.Branch);

        ApplyBranchScopedFilter<OrderItem>(modelBuilder, e => e.Order.Branch);

        ApplyBranchScopedFilter<Payment>(modelBuilder, e => e.Order.Branch);

        ApplyBranchScopedFilter<InventoryTransaction>(modelBuilder, e => e.Inventory.Branch);
    }


    private void ApplyDirectTenantAndBranchFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity, IBranchOwnedDirect
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var contextExpression = Expression.Constant(this);


        var orgProp = Expression.Property(parameter, nameof(ITenantEntity.OrganizationId));
        var currentOrgId = Expression.Property(contextExpression, nameof(CurrentOrganizationId));
        var tenantCheck = Expression.Equal(Expression.Convert(orgProp, typeof(int?)), currentOrgId);

        // Null-safe: never read Nullable.Value inside a query filter (EF evaluates it
        // eagerly, which throws when the tenant/branch claim is absent).
        var branchProp = Expression.Property(parameter, nameof(IBranchOwnedDirect.BranchId));
        var currentBranchId = Expression.Property(contextExpression, nameof(CurrentBranchId));
        var branchNotRestricted = Expression.Equal(currentBranchId, Expression.Constant(null, typeof(int?)));
        var branchMatches = Expression.Equal(Expression.Convert(branchProp, typeof(int?)), currentBranchId);
        var branchCheck = Expression.OrElse(branchNotRestricted, branchMatches);

        var body = Expression.AndAlso(tenantCheck, branchCheck);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        modelBuilder.Entity<TEntity>().HasQueryFilter(lambda);
    }

    private void ApplyBranchScopedFilter<TEntity>(ModelBuilder modelBuilder, Expression<Func<TEntity, Branch>> branchSelector) where TEntity : class
    {
        var parameter = branchSelector.Parameters[0];

        var branchAccess = branchSelector.Body;

        var branchOrganizationId = Expression.Property(branchAccess, nameof(Branch.OrganizationId));

        var contextExpression = Expression.Constant(this);

        var currentOrganizationId = Expression.Property(contextExpression, nameof(CurrentOrganizationId));

        var organizationMatches = Expression.Equal(
            Expression.Convert(branchOrganizationId, typeof(int?)),currentOrganizationId);

        var tenantCheck = organizationMatches;

        var branchId = Expression.Property(branchAccess, nameof(Branch.BranchId));

        var currentBranchId = Expression.Property(contextExpression, nameof(CurrentBranchId));

        // Null-safe branch comparison (see BuildTenantFilter for why Nullable.Value is avoided).
        var branchNotRestricted = Expression.Equal(currentBranchId, Expression.Constant(null, typeof(int?)));

        var branchMatches = Expression.Equal(Expression.Convert(branchId, typeof(int?)), currentBranchId);

        var branchCheck = Expression.OrElse(branchNotRestricted, branchMatches);

        var body = Expression.AndAlso(tenantCheck, branchCheck);

        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        modelBuilder.Entity<TEntity>().HasQueryFilter(lambda);
    }



    private void ApplyDirectBranchScopedFilter<TEntity>(
    ModelBuilder modelBuilder,
    Expression<Func<TEntity, int>> branchIdSelector)
    where TEntity : class
    {
        var parameter = branchIdSelector.Parameters[0];

        var branchIdProperty = branchIdSelector.Body;

        var contextExpression =
            Expression.Constant(this);

        var currentBranchId =
            Expression.Property(
                contextExpression,
                nameof(CurrentBranchId));

        var branchHasValue =
            Expression.Property(
                currentBranchId,
                nameof(Nullable<int>.HasValue));

        var branchNotRestricted =
            Expression.Not(branchHasValue);

        var branchValue =
            Expression.Property(
                currentBranchId,
                nameof(Nullable<int>.Value));

        var branchMatches =
            Expression.Equal(
                branchIdProperty,
                branchValue);

        var branchCheck =
            Expression.OrElse(
                branchNotRestricted,
                branchMatches);

        var lambda =
            Expression.Lambda<Func<TEntity, bool>>(
                branchCheck,
                parameter);

        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(lambda);
    }

    // =========================================================
    // Transaction
    // =========================================================

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await Database.BeginTransactionAsync();
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
