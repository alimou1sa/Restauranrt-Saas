using RestaurantSaaS.Application.DTOs.Branches.BranchRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{
    public interface IBranchService
    {
        Task<BranchResponse> CreateAsync(int organizationId, CreateBranchRequest request);

        Task<BranchResponse?> GetByIdAsync(int id);

        Task<List<BranchResponse>> GetAllByOrganizationAsync(int organizationId);

        Task<BranchResponse?> UpdateAsync(int id,UpdateBranchRequest request);

        Task<bool> DeleteAsync(int id);
    }
}
