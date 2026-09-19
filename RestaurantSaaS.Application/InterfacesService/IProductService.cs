using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Products.ProductsRequest;
using RestaurantSaaS.Application.DTOs.Products.ProductsResponse;
namespace RestaurantSaaS.Application.InterfacesService
{


    public interface IProductService
    {

        Task<ProductDetailsResponse> CreateAsync(int categoryId, CreateProductRequest request);

        Task<ProductDetailsResponse?> GetByIdAsync(int productId);

        Task<List<ProductListResponse>> GetAllByBranchAsync(int branchId);

        Task<ProductDetailsResponse?> UpdateAsync(int productId, UpdateProductRequest request);

        Task<ProductDetailsResponse?> UpdateAvailabilityAsync(int productId, UpdateProductAvailabilityRequest request);

        Task<bool> DeleteAsync(int productId);
    }
}
