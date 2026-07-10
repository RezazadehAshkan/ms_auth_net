using System;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;
namespace AuthenticationService.Application.Users.Signup
{
    
    public class SignupService{
            private readonly IUserRepository _users;
            public SignupService(IUserRepository users)
                {
                    _users = users;
                }

        public async Task<SignupResponse> ExecuteAsync(SignupRequest request)
        {
            var existing = await _users.UserExistsByEmailAsync(request.Email);

            if (existing)
                throw new Exception("Email already exists");

            var user = new User(
                request.Username,
                request.Email,
                request.Password
            );

            var userId = await _users.AddUser(user);

            return new SignupResponse(userId);
        }
    }
}