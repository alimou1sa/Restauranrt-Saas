using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Users.UserResponse;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Services
{
    public class PlatformUserService : IPlatformUserService
    {
        private readonly IAppDbContext _context;
        public PlatformUserService(IAppDbContext context) => _context = context;

        public async Task<List<UserResponse>> GetAllAsync()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            return users.Select(ToResponse).ToList();
        }

        public async Task<UserResponse?> GetByIdAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            return user is null ? null : ToResponse(user);
        }

        public async Task<UserResponse?> SetActiveAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return null;

            user.IsActive = isActive;
            user.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ToResponse(user);
        }

        private static UserResponse ToResponse(User u) => new()
        {
            UserId = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Phone = u.Phone,
            IsActive = u.IsActive,
            EmailConfirmed = u.EmailConfirmed,
            LastLoginAtUtc = u.LastLoginAtUtc,
            CreatedAtUtc = u.CreatedAtUtc,
            UpdatedAtUtc = u.UpdatedAtUtc
        };
    }
}
