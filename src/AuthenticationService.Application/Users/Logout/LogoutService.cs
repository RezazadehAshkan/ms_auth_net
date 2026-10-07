using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Application.Users.Logout
{

    public class LogoutService
    {
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LogoutService> _logger;

        public LogoutService(IRefreshTokenRepository refreshTokens, ITokenService tokenService, ILogger<LogoutService> logger)
        {
            _refreshTokens = refreshTokens;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<LogoutResponse?> LogoutAsync(LogoutRequest request)
        {
            _logger.LogInformation("Starting logout flow");
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Logout flow failed because the refresh token was missing");
                return null;
            }

            // Tokens are only ever stored/compared by hash, never in their raw form.
            var hashedToken = await _tokenService.HashTokenAsync(request.RefreshToken);
            var refreshToken = await _refreshTokens.GetRefreshTokenAsync(hashedToken);
            if (refreshToken == null)
            {
                // Treat unknown tokens as a successful logout so the endpoint
                // cannot be used to probe which refresh tokens are still valid.
                _logger.LogInformation("Logout flow completed because no session was found for the refresh token");
                return new LogoutResponse("Logged out successfully.");
            }

            if (!refreshToken.IsActive)
            {
                _logger.LogInformation("Logout flow completed because the session was already inactive for user ID {UserId}", refreshToken.UserId);
                return new LogoutResponse("Logged out successfully.");
            }

            refreshToken.Revoke();
            await _refreshTokens.UpdateRefreshTokenAsync(refreshToken);

            _logger.LogInformation("Logout flow completed and refresh token revoked for user ID {UserId}", refreshToken.UserId);
            return new LogoutResponse("Logged out successfully.");
        }
    }
}