using System;
using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;

namespace AuthenticationService.Application.Users.Signup
{
    
    public class SignupService{
            private readonly IUserRepository _users;
            private readonly IPasswordHasher _passwordHasher;
            public SignupService(IUserRepository users, IPasswordHasher passwordHasher)
                {
                    _users = users;
                    _passwordHasher = passwordHasher;
                }

        public async Task<SignupResponse> ExecuteAsync(SignupRequest request)
        {
            var existing = await _users.UserExistsByEmailAsync(request.Email);

            if (existing)
                throw new Exception("Email already exists");

            var user = new User(
                request.Username,
                request.Email,
                _passwordHasher.HashPassword(request.Password)
            );

            var userId = await _users.AddUser(user);

            return new SignupResponse(userId);
        }
    }
}