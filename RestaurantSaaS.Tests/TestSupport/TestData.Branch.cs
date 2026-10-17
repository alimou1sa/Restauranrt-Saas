using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Data;

namespace RestaurantSaaS.Tests.TestSupport;

public static partial class TestData
{
    private static async Task<BranchSeed> SeedBranchGraphAsync(AppDbContext ctx, Organization org, Branch branch, string key)
    {
        var seed = new BranchSeed { BranchId = branch.BranchId };

        var menu = new Menu
        {
            BranchId = branch.BranchId,
            Name = $"Menu {key}",
            Description = $"Menu for {key}",
            IsActive = true,
            IsPublished = true,
            CreatedAtUtc = Now
        };
        var table = new RestaurantTable
        {
            BranchId = branch.BranchId,
            TableNumber = $"T-{key}",
            Capacity = 2,
            QRCodeToken = Guid.NewGuid(),
            IsActive = true,
            CreatedAtUtc = Now
        };
        ctx.AddRange(menu, table);
        await ctx.SaveChangesAsync();
        seed.MenuId = menu.MenuId;
        seed.TableId = table.TableId;

        var category = new Category
        {
            MenuId = menu.MenuId,
            Name = $"Category {key}",
            DisplayOrder = 0,
            IsActive = true
        };
        ctx.Add(category);
        await ctx.SaveChangesAsync();
        seed.CategoryId = category.CategoryId;

        var product = new Product
        {
            OrganizationId = org.OrganizationId,
            CategoryId = category.CategoryId,
            Name = $"Product {key}",
            Price = 10m,
            IsActive = true,
            IsAvailable = true,
            DisplayOrder = 0,
            CreatedAtUtc = Now
        };
        ctx.Add(product);
        await ctx.SaveChangesAsync();
        seed.ProductId = product.ProductId;

        var order = new Order
        {
            OrganizationId = org.OrganizationId,
            BranchId = branch.BranchId,
            TableId = table.TableId,
            OrderNumber = $"ORD-{key}",
            Status = "Pending",
            OrderType = "DineIn",
            SubTotal = 20m,
            DiscountAmount = 0m,
            TaxAmount = 0m,
            TotalAmount = 20m,
            CreatedAtUtc = Now
        };
        ctx.Add(order);
        await ctx.SaveChangesAsync();
        seed.OrderId = order.OrderId;

        var orderItem = new OrderItem
        {
            OrderId = order.OrderId,
            ProductId = product.ProductId,
            ProductName = product.Name,
            UnitPrice = 10m,
            Quantity = 2m,
            DiscountAmount = 0m
        };
        var payment = new Payment
        {
            OrderId = order.OrderId,
            Amount = 20m,
            PaymentMethod = "Cash",
            Status = "Paid",
            CreatedAtUtc = Now
        };
        var inventory = new Inventory
        {
            BranchId = branch.BranchId,
            ProductId = product.ProductId,
            Quantity = 100m,
            ReorderLevel = 10m,
            UpdatedAtUtc = Now
        };
        ctx.AddRange(orderItem, payment, inventory);
        await ctx.SaveChangesAsync();
        seed.OrderItemId = orderItem.OrderItemId;
        seed.PaymentId = payment.PaymentId;
        seed.InventoryId = inventory.InventoryId;

        var transaction = new InventoryTransaction
        {
            InventoryId = inventory.InventoryId,
            TransactionType = "Adjustment",
            Quantity = 5m,
            CreatedAtUtc = Now
        };
        ctx.Add(transaction);
        await ctx.SaveChangesAsync();
        seed.InventoryTransactionId = transaction.InventoryTransactionId;

        return seed;
    }
}
