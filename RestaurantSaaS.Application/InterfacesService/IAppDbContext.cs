using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{
  
    public interface IAppDbContext
    {
        DbSet<Organization> Organizations { get; }

        DbSet<User> Users { get; }

        DbSet<Branch> Branches { get; }

        DbSet<OrganizationUser> OrganizationUsers { get; }

        DbSet<Permission> Permissions { get; }

        DbSet<Role> Roles { get; }

        DbSet<UserRole> UserRoles { get; }

        DbSet<Menu> Menus { get; }
        DbSet<Category> Categories { get; }

        DbSet<Product> Products {  get; }

        DbSet<RestaurantTable> RestaurantTables { get; }

        DbSet<RolePermission> RolePermissions { get; }
        DbSet<Inventory>  Inventories { get; }
        DbSet<InventoryTransaction> InventoryTransactions { get; }
        DbSet<Plan> Plans { get; }  

        DbSet<Subscription> Subscriptions { get; }  
        
        DbSet<Payment> Payments { get; }

        DbSet<Order> Orders { get; }

        DbSet<Customer> Customers { get; }  
        DbSet<OrderItem> OrderItems { get; }

        DbSet<SystemRole> SystemRoles { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
