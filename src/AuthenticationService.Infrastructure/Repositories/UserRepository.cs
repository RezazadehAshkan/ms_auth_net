using System;
using AuthenticationService.Domain.Repositories;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace AuthenticationService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthenticationDbContext _dbContext;
        public UserRepository(AuthenticationDbContext dbContext)
        {
            // Initialize any required dependencies, such as a database context
            _dbContext = dbContext;
        }
        // Implementation of the UserRepository class
        public async Task<Guid> AddUser(User user)
        {
            // Implement the logic to add a user to the database
            // For example, using Entity Framework Core:
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return user.Id; // Assuming User has an Id property of type Guid
        }
        public async Task<bool> UserExistsByEmailAsync(string email)
        {

            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        public async Task UpdateUserAsync(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}