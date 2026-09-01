using System;
using AuthenticationService.Domain.Repositories;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace AuthenticationService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthenticationDbContext _dbContext;
        private readonly ILogger<RefreshTokenRepository> _logger;
        public RefreshTokenRepository(AuthenticationDbContext dbContext, ILogger<RefreshTokenRepository> logger)
        {
            // Initialize any required dependencies, such as a database context
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<Guid> AddRefreshToken(RefreshToken refreshToken)
        {
            _logger.LogInformation("Adding refresh token for user ID {UserId}", refreshToken.UserId);
            try { await _dbContext.RefreshTokens.AddAsync(refreshToken); await _dbContext.SaveChangesAsync(); return refreshToken.Id; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to add refresh token for user ID {UserId}", refreshToken.UserId); throw new InvalidOperationException("Failed to add refresh token.", ex); }
        }
        public async Task<RefreshToken?> GetRefreshTokenAsync(string hashedRefreshToken)
        {
            _logger.LogInformation("Getting refresh token by hash");
            try
            {
                return await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == hashedRefreshToken);
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to get refresh token by hash"); throw new InvalidOperationException("Failed to get refresh token.", ex); }
        }
        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _logger.LogInformation("Updating refresh token ID {TokenId}", refreshToken.Id);
            try { _dbContext.RefreshTokens.Update(refreshToken); await _dbContext.SaveChangesAsync(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to update refresh token ID {TokenId}", refreshToken.Id); throw new InvalidOperationException("Failed to update refresh token.", ex); }
        }
        public async Task<RefreshToken?> GetLastRefreshTokenByUserIdAsync(Guid userId, Guid? excludeTokenId = null)
        {
            _logger.LogInformation("Getting latest refresh token for user ID {UserId}", userId);
            var query = _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId);

            if (excludeTokenId.HasValue)
            {
                var exclude = excludeTokenId.Value;
                query = query.Where(rt => rt.Id != exclude);
            }

            try
            {
                return await query
                .OrderByDescending(rt => rt.ExpiresAt)
                .FirstOrDefaultAsync();
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to get latest refresh token for user ID {UserId}", userId); throw new InvalidOperationException("Failed to get latest refresh token.", ex); }
        }

    }
}