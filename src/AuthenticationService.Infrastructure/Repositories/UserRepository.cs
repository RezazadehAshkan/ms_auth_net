using System;
using AuthenticationService.Domain.Repositories;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace AuthenticationService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthenticationDbContext _dbContext;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(AuthenticationDbContext dbContext, ILogger<UserRepository> logger)
        {
            // Initialize any required dependencies, such as a database context
            _dbContext = dbContext;
            _logger = logger;
        }
        // Implementation of the UserRepository class
        public async Task<Guid> AddUser(User user)
        {
            _logger.LogInformation("Adding user {Username}", user.Username);
            // Implement the logic to add a user to the database
            // For example, using Entity Framework Core:
            try
            {
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("User {Username} added with ID {UserId}", user.Username, user.Id);
                return user.Id; // Assuming User has an Id property of type Guid
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding user");
                throw new InvalidOperationException("Failed to add user.", ex);
            }
        }
        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            _logger.LogInformation("Checking whether a user exists for email {Email}", email);
            try { return await _dbContext.Users.AnyAsync(u => u.Email == email); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to check user existence for email {Email}", email); throw new InvalidOperationException("Failed to check user existence.", ex); }
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            _logger.LogInformation("Getting user by username {Username}", username);
            try { return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to get user by username {Username}", username); throw new InvalidOperationException("Failed to get user by username.", ex); }
        }
        public async Task UpdateUserAsync(User user)
        {
            _logger.LogInformation("Updating user ID {UserId}", user.Id);
            try { _dbContext.Users.Update(user); await _dbContext.SaveChangesAsync(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to update user ID {UserId}", user.Id); throw new InvalidOperationException("Failed to update user.", ex); }
        }
        public async Task<string?> GetUserIdByEmailAsync(string Email)
        {
            _logger.LogInformation("Getting user ID by email {Email}", Email);
            try
            {
                return await _dbContext.Users
                .Where(u => u.Email == Email)
                .Select(u => u.Id.ToString())
                .FirstOrDefaultAsync();
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to get user ID by email {Email}", Email); throw new InvalidOperationException("Failed to get user ID by email.", ex); }
        }
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            _logger.LogInformation("Getting user by ID {UserId}", userId);
            try { return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to get user by ID {UserId}", userId); throw new InvalidOperationException("Failed to get user by ID.", ex); }
        }
    }
}