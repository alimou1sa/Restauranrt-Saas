using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Plans.PlansRequest;
    using global::RestaurantSaaS.Application.DTOs.Plans.PlansResponse;
namespace RestaurantSaaS.Application.InterfacesService
{

    // Plan is a platform-level catalog (see PlanResponse remarks) - in
    // practice these endpoints should be restricted to a system/platform-admin
    // role, same as IPermissionService.
    public interface IPlanService
    {
        Task<PlanResponse> CreateAsync(CreatePlanRequest request);

        Task<PlanResponse?> GetByIdAsync(int planId);

        Task<List<PlanResponse>> GetAllAsync();

        Task<PlanResponse?> UpdateAsync(int planId, UpdatePlanRequest request);

        Task<bool> DeleteAsync(int planId);
    }
}
