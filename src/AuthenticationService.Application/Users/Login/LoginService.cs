using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;

namespace AuthenticationService.Application.Users.Login
{
    
    public class LoginService{
            private readonly IUserRepository _users;
            private readonly IPasswordHasher _passwordHasher;
            private readonly ITokenService _tokenService;

            // private readonly int _tokenExpirationMs;
            public LoginService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService/*, int tokenExpirationMs*/)
                {
                    _users = users;
                    _passwordHasher = passwordHasher;
                    _tokenService = tokenService;
                    // _tokenExpirationMs = tokenExpirationMs;

                }
                public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _users.GetUserByUsernameAsync(request.Username);

            if (user == null)
                return null;

            var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

            if (!isPasswordValid)
                return null;

            //JWT
            // Generate a JWT token for the authenticated user
            var token = _tokenService.GenerateJwtToken(user.Id.ToString(), user.Username, user.Email);
            user.UpdateLastLogin();
            await _users.UpdateUserAsync(user);

            return new LoginResponse(token,token, 1);
        }

    }
}