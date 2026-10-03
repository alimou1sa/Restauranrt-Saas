using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RestaurantSaaS.Application.Common;
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

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasQueryFilter(BuildTenantFilter(clrType));
            }
        }
    }


    private LambdaExpression BuildTenantFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");

        var organizationIdProperty =Expression.Property(parameter,nameof(ITenantEntity.OrganizationId));

        var contextExpression =Expression.Constant(this);

        var currentOrganizationId =Expression.Property(contextExpression,nameof(CurrentOrganizationId));

        var hasValue =Expression.Property(currentOrganizationId,nameof(Nullable<int>.HasValue));

        var value =Expression.Property(currentOrganizationId,nameof(Nullable<int>.Value));


        var organizationMatches =Expression.Equal(organizationIdProperty,value);


        var body =Expression.AndAlso(hasValue,organizationMatches);

        return Expression.Lambda(body,parameter);
    }


    // =========================================================
    // Branch Scoped Filters
    // =========================================================

    private void ApplyBranchScopedFilters(ModelBuilder modelBuilder)
    {

        ApplyBranchScopedFilter<Menu>(modelBuilder,e => e.Branch);

        ApplyBranchScopedFilter<RestaurantTable>(modelBuilder,e => e.Branch);

        ApplyBranchScopedFilter<Inventory>(modelBuilder,e => e.Branch);



        ApplyDirectBranchScopedFilter<Order>(modelBuilder,e => e.BranchId);

        ApplyBranchScopedFilter<Category>(modelBuilder,e => e.Menu.Branch);
     ApplyBranchScopedFilter<Product>(modelBuilder, e => e.Category.Menu.Branch);
        ApplyBranchScopedFilter<OrderItem>(modelBuilder,e => e.Order.Branch);

        ApplyBranchScopedFilter<Payment>(modelBuilder,e => e.Order.Branch);

        ApplyBranchScopedFilter<InventoryTransaction>(modelBuilder,e => e.Inventory.Branch);
    }


    private void ApplyBranchScopedFilter<TEntity>(ModelBuilder modelBuilder,Expression<Func<TEntity, Branch>> branchSelector)where TEntity : class
    {
        var parameter = branchSelector.Parameters[0];

        var branchAccess = branchSelector.Body;

        var branchOrganizationId =Expression.Property(branchAccess,nameof(Branch.OrganizationId));

        var contextExpression =Expression.Constant(this);

        var currentOrganizationId =Expression.Property(contextExpression,nameof(CurrentOrganizationId));

        var organizationHasValue =Expression.Property(currentOrganizationId,nameof(Nullable<int>.HasValue));

        var organizationValue =Expression.Property(currentOrganizationId,nameof(Nullable<int>.Value));

        var organizationMatches =Expression.Equal(branchOrganizationId,organizationValue);

        var tenantCheck =Expression.AndAlso(organizationHasValue,organizationMatches);

        var branchId =Expression.Property(branchAccess,nameof(Branch.BranchId));

        var currentBranchId =Expression.Property(contextExpression,nameof(CurrentBranchId));

        var branchHasValue =Expression.Property(currentBranchId,nameof(Nullable<int>.HasValue));

        var branchNotRestricted =Expression.Not(branchHasValue);

        var branchValue =Expression.Property(currentBranchId,nameof(Nullable<int>.Value));

        var branchMatches =Expression.Equal(branchId,branchValue);

        var branchCheck =Expression.OrElse(branchNotRestricted,branchMatches);

        var body =Expression.AndAlso(tenantCheck,branchCheck);

        var lambda =Expression.Lambda<Func<TEntity, bool>>(body,parameter);

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
