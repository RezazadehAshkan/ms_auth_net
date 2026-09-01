using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Application.Users.Login
{

    public class LoginService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenFactory _refreshTokenFactory;
        private readonly ILogger<LoginService> _logger;

        private readonly int _tokenExpirationMs = 15 * 60 * 1000; // 15 minutes in milliseconds
        public LoginService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService, IRefreshTokenFactory refreshTokenFactory, ILogger<LoginService> logger)
        {
            _users = users;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokenFactory = refreshTokenFactory;
            _logger = logger;
        }
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Starting login for username {Username}", request.Username);
            _logger.LogWarning("Starting login for username {Username}", request.Username);

            var user = await _users.GetUserByUsernameAsync(request.Username);

            if (user == null)
            {
                _logger.LogInformation("Login failed because user {Username} was not found", request.Username);
                return null;
            }

            var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                _logger.LogInformation("Login failed because the password was invalid for username {Username}", request.Username);
                return null;
            }

            var accessToken = _tokenService.GenerateJwtToken(user.Id.ToString(), user.Username, user.Email);
            var unHashedNewRefreshToken = (await _refreshTokenFactory.CreateAsync(user.Id)).unHashedToken;
            user.UpdateLastLogin();
            await _users.UpdateUserAsync(user);
            var expirationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _tokenExpirationMs;
            _logger.LogInformation("Login completed for username {Username}", request.Username);
            return new LoginResponse(accessToken, unHashedNewRefreshToken, expirationTimestamp);
        }

    }
}