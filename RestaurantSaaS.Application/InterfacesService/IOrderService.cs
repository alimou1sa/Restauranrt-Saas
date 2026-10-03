    using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
    using global::RestaurantSaaS.Application.DTOs.Orders.OrdersRequest;
    using global::RestaurantSaaS.Application.DTOs.Orders.OrdersResponse;
using RestaurantSaaS.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IOrderService
    {
        Task<OrderDetailsResponse> CreateAsync(int branchId, CreateOrderRequest request);

        Task<OrderDetailsResponse?> GetByIdAsync(int orderId);


        Task<List<OrderListResponse>> GetAllAsync();

        Task<OrderDetailsResponse?> UpdateAsync(int orderId, UpdateOrderRequest request);


        Task<OrderDetailsResponse?> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request);

        Task<bool> DeleteAsync(int orderId);
    }
}
