using System;
using AuthenticationService.Domain.Entities;

namespace AuthenticationService.Domain.Repositories
{
    public interface IUserRepository
    {
        // Define methods for user repository operations
        Task<Guid> AddUser(User user);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task UpdateUserAsync(User user);
        Task<string?> GetUserIdByEmailAsync(string Email);

        Task<User?> GetUserByIdAsync(string userId);
        //Task<User> GetUserByUsernameAsync(string username);

        //Task DeleteUserAsync(Guid userId);
    }
}