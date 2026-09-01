using System;
using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;
using Microsoft.Extensions.Logging;
namespace AuthenticationService.Infrastructure.Services
{
    public sealed class RefreshTokenFactory : IRefreshTokenFactory
    {
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<RefreshTokenFactory> _logger;
        private readonly long refreshTokenExpirationMs = 7 * 24 * 60 * 60 * 1000; // 7 days in milliseconds

        public RefreshTokenFactory(ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository, ILogger<RefreshTokenFactory> logger)
        {
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }
        public async Task<(RefreshToken? refToken, string? unHashedToken)> CreateAsync(Guid userId, RefreshToken? latestRefreshToken = null)
        {
            _logger.LogInformation("Creating refresh token for user ID {UserId}", userId);
            var unHashedNewRefreshToken = _tokenService.GenerateRefreshToken();
            var hashedNewRefreshToken = await _tokenService.HashTokenAsync(unHashedNewRefreshToken);
            var newRefreshToken = new RefreshToken(
                userId: userId,
                hashedToken: hashedNewRefreshToken,
                expiresAt: DateTime.UtcNow.AddMilliseconds(refreshTokenExpirationMs)
            );
            await _refreshTokenRepository.AddRefreshToken(newRefreshToken);
            // I want this task to run in the background later
            await UpdateOldRefreshTokenAsync(newRefreshToken, latestRefreshToken);
            _logger.LogInformation("Refresh token created for user ID {UserId}", userId);
            return (newRefreshToken, unHashedNewRefreshToken);
        }
        public async Task<bool> UpdateOldRefreshTokenAsync(RefreshToken newRefreshToken, RefreshToken? latestRefreshToken = null)
        {
            _logger.LogInformation("Updating previous refresh token for user ID {UserId}", newRefreshToken.UserId);
            latestRefreshToken ??= await _refreshTokenRepository.GetLastRefreshTokenByUserIdAsync(newRefreshToken.UserId, newRefreshToken.Id);
            if (latestRefreshToken == null)
            {
                _logger.LogInformation("No previous refresh token found for user ID {UserId}", newRefreshToken.UserId);
                return true;
            }
            try
            {
                latestRefreshToken.ReplaceWith(newRefreshToken.Id);
                await _refreshTokenRepository.UpdateRefreshTokenAsync(latestRefreshToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update previous refresh token for user ID {UserId}", newRefreshToken.UserId);
                return false;
            }

        }
    }
}