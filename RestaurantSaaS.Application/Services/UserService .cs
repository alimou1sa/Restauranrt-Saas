using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantSaaS.Application.DTOs.Users.UserRequest;
using RestaurantSaaS.Application.DTOs.Users.UserResponse;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Users;


namespace RestaurantSaaS.Application.Services
{

    public class UserService : IUserService
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

     
        public UserService(IAppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email);

            if (emailExists)
                throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Phone = request.Phone,
                IsActive = true,
                EmailConfirmed = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return ToResponse(user);
        }
    
        public async Task<UserResponse?> GetByIdAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user is null ? null : ToResponse(user);
        }
        public async Task<UserResponse?> GetByEmailAsync(string Email)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == Email);

            return user is null ? null : ToResponse(user);
        }

        public async Task<List<UserResponse>> GetAllAsync()
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            return users.Select(ToResponse).ToList();
        }

        public async Task<UserResponse?> UpdateAsync(int userId, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user is null)
                return null;

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToResponse(user);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return false;

            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            user.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        private static UserResponse ToResponse(User user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                LastLoginAtUtc = user.LastLoginAtUtc,
                CreatedAtUtc = user.CreatedAtUtc,
                UpdatedAtUtc = user.UpdatedAtUtc
            };
        }
   
    
    }
}
