using System;
using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;
namespace AuthenticationService.Infrastructure.Services
{
    public sealed class RefreshTokenFactory : IRefreshTokenFactory
    {
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly long refreshTokenExpirationMs = 7 * 24 * 60 * 60 * 1000; // 7 days in milliseconds

        public RefreshTokenFactory(ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
        {
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
        }
        public async Task<(RefreshToken? refToken, string? unHashedToken)> CreateAsync(Guid userId, RefreshToken? latestRefreshToken = null)
        {
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
            return (newRefreshToken, unHashedNewRefreshToken);
        }
        public async Task<bool> UpdateOldRefreshTokenAsync(RefreshToken newRefreshToken, RefreshToken? latestRefreshToken = null)
        {
            latestRefreshToken ??= await _refreshTokenRepository.GetLastRefreshTokenByUserIdAsync(newRefreshToken.UserId, newRefreshToken.Id);
            if (latestRefreshToken == null)
            {
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
                //Log
                return false;
            }

        }
    }
}