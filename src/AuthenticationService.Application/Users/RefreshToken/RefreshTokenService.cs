using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Application.Users.RefreshToken
{

    public class RefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenFactory _refreshTokenFactory;
        private readonly ILogger<RefreshTokenService> _logger;
        public RefreshTokenService(IRefreshTokenRepository refreshTokens, ITokenService tokenService, IRefreshTokenFactory refreshTokenFactory, ILogger<RefreshTokenService> logger)
        {
            _refreshTokens = refreshTokens;
            _tokenService = tokenService;
            _refreshTokenFactory = refreshTokenFactory;
            _logger = logger;
        }
        public async Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            _logger.LogInformation("Starting refresh token flow");
            string hashedToken = await _tokenService.HashTokenAsync(request.RefreshToken);
            var oldRefreshToken = await _refreshTokens.GetRefreshTokenAsync(hashedToken);
            if (oldRefreshToken == null)
            {
                _logger.LogInformation("Refresh token flow failed because the token was not found");
                return null;
            }
            if (!oldRefreshToken.IsActive)
            {
                _logger.LogInformation("Refresh token flow failed because the token is inactive");
                return null;
            }
            var newAccessToken = _tokenService.GenerateJwtToken(oldRefreshToken.UserId.ToString(), oldRefreshToken.User.Username, oldRefreshToken.User.Email);
            //maybe it is better not to pass the old refresh token to the factory because of race condition, but it is a trade-off between performance and security. If we don't pass the old refresh token, we will have to query the database again to get the latest refresh token, which is an extra database call.

            var unHashedNewRefreshToken = (await _refreshTokenFactory.CreateAsync(oldRefreshToken.UserId, oldRefreshToken)).unHashedToken;

            _logger.LogInformation("Refresh token flow completed for user ID {UserId}", oldRefreshToken.UserId);
            return new RefreshTokenResponse(newAccessToken, unHashedNewRefreshToken);
        }
    }
}