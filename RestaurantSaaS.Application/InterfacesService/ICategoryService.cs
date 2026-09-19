using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Categories.CategoriesRequest;
    using global::RestaurantSaaS.Application.DTOs.Categories.CategoriesResponse;
namespace RestaurantSaaS.Application.InterfacesService
{


    public interface ICategoryService
    {

        Task<CategoryResponse> CreateAsync(int menuId, CreateCategoryRequest request);

        Task<CategoryResponse?> GetByIdAsync(int categoryId);

        Task<List<CategoryResponse>> GetAllByMenuAsync(int menuId);

        Task<CategoryResponse?> UpdateAsync(int categoryId, UpdateCategoryRequest request);

        Task<bool> DeleteAsync(int categoryId);
    }
}
