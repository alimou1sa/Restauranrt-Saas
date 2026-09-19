using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
    using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsResponse;
namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IOrderItemService
    {

        Task<OrderItemResponse> AddAsync(int orderId, CreateOrderItemRequest request);

        Task<List<OrderItemResponse>> GetAllByOrderAsync(int orderId);


        Task<OrderItemResponse?> UpdateAsync(int orderItemId, UpdateOrderItemRequest request);

        Task<bool> RemoveAsync(int orderItemId);
    }
}
