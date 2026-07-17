using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;

namespace AuthenticationService.Application.Users.Login
{

    public class LoginService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        private readonly IRefreshTokenRepository _refreshTokens;

        // private readonly int _tokenExpirationMs;
        public LoginService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService, IRefreshTokenRepository refreshTokens)
        {
            _users = users;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokens = refreshTokens;
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

            var accessToken = _tokenService.GenerateJwtToken(user.Id.ToString(), user.Username, user.Email);

            var latestRefreshToken = await _refreshTokens.GetLastRefreshTokenByUserIdAsync(user.Id);


            var unHashedNewRefreshToken = _tokenService.GenerateRefreshToken();
            var hashedNewRefreshToken = await _tokenService.HashTokenAsync(unHashedNewRefreshToken);
            var newRefreshToken = new AuthenticationService.Domain.Entities.RefreshToken(
                userId: user.Id,
                hashedToken: hashedNewRefreshToken,
                expiresAt: DateTime.UtcNow.AddDays(7)
            );
            user.UpdateLastLogin();
            await _users.UpdateUserAsync(user);
            await _refreshTokens.AddRefreshToken(newRefreshToken);
            if (latestRefreshToken != null)
            {
                latestRefreshToken.ReplaceWith(newRefreshToken.Id);
                await _refreshTokens.UpdateRefreshTokenAsync(latestRefreshToken);
            }

            return new LoginResponse(accessToken, unHashedNewRefreshToken, 1);
        }

    }
}