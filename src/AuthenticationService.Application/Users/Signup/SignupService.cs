using System;
using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Application.Users.Signup
{

    public class SignupService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<SignupService> _logger;
        public SignupService(IUserRepository users, IPasswordHasher passwordHasher, ILogger<SignupService> logger)
        {
            _users = users;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<SignupResponse> ExecuteAsync(SignupRequest request)
        {
            _logger.LogInformation("Starting signup for username {Username}", request.Username);
            var user = new User(
                request.Username,
                request.Email,
                _passwordHasher.HashPassword(request.Password)
            );
            try
            {
                var userId = await _users.AddUser(user);

                _logger.LogInformation("Signup completed for username {Username} with user ID {UserId}", request.Username, userId);
                return new SignupResponse(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user for username {Username}", request.Username);
                throw new InvalidOperationException("Error creating user", ex);
            }
        }
    }
}