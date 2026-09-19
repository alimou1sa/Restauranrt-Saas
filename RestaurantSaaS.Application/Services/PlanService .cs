using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
  using global::RestaurantSaaS.Application.DTOs.Plans.PlansRequest;
    using global::RestaurantSaaS.Application.DTOs.Plans.PlansResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;

namespace RestaurantSaaS.Application.Services
{


    public class PlanService : IPlanService
    {
        private readonly IAppDbContext _context;

        public PlanService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<PlanResponse> CreateAsync(CreatePlanRequest request)
        {
            var nameExists = await _context.Plans.AnyAsync(p => p.Name == request.Name);
            if (nameExists)
                throw new InvalidOperationException($"A plan named '{request.Name}' already exists.");

            var plan = new Plan
            {
                Name = request.Name,
                Description = request.Description,
                MonthlyPrice = request.MonthlyPrice,
                YearlyPrice = request.YearlyPrice,
                MaxBranches = request.MaxBranches,
                MaxUsers = request.MaxUsers,
                MaxProducts = request.MaxProducts,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            return ToResponse(plan);
        }

        public async Task<PlanResponse?> GetByIdAsync(int planId)
        {
            var plan = await _context.Plans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlanId == planId);

            return plan is null ? null : ToResponse(plan);
        }

        public async Task<List<PlanResponse>> GetAllAsync()
        {
            var plans = await _context.Plans
                .AsNoTracking()
                .ToListAsync();

            return plans.Select(ToResponse).ToList();
        }

        public async Task<PlanResponse?> UpdateAsync(int planId, UpdatePlanRequest request)
        {
            var plan = await _context.Plans.FindAsync(planId);
            if (plan is null)
                return null;

            var nameExists = await _context.Plans
                .AnyAsync(p => p.Name == request.Name && p.PlanId != planId);

            if (nameExists)
                throw new InvalidOperationException($"A plan named '{request.Name}' already exists.");

            plan.Name = request.Name;
            plan.Description = request.Description;
            plan.MonthlyPrice = request.MonthlyPrice;
            plan.YearlyPrice = request.YearlyPrice;
            plan.MaxBranches = request.MaxBranches;
            plan.MaxUsers = request.MaxUsers;
            plan.MaxProducts = request.MaxProducts;
            plan.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return ToResponse(plan);
        }

        public async Task<bool> DeleteAsync(int planId)
        {
            var plan = await _context.Plans.FindAsync(planId);
            if (plan is null)
                return false;

            _context.Plans.Remove(plan);
            await _context.SaveChangesAsync();

            return true;
        }

        private static PlanResponse ToResponse(Plan plan)
        {
            return new PlanResponse
            {
                PlanId = plan.PlanId,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                YearlyPrice = plan.YearlyPrice,
                MaxBranches = plan.MaxBranches,
                MaxUsers = plan.MaxUsers,
                MaxProducts = plan.MaxProducts,
                IsActive = plan.IsActive,
                CreatedAtUtc = plan.CreatedAtUtc
            };
        }
    }
}
