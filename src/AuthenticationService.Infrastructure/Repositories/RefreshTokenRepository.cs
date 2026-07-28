using System;
using AuthenticationService.Domain.Repositories;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace AuthenticationService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthenticationDbContext _dbContext;
        public RefreshTokenRepository(AuthenticationDbContext dbContext)
        {
            // Initialize any required dependencies, such as a database context
            _dbContext = dbContext;
        }
        public async Task<Guid> AddRefreshToken(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
            return refreshToken.Id;
        }
        public async Task<RefreshToken?> GetRefreshTokenAsync(string hashedRefreshToken)
        {
            return await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == hashedRefreshToken);
        }
        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Update(refreshToken);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<RefreshToken?> GetLastRefreshTokenByUserIdAsync(Guid userId, Guid? excludeTokenId = null)
        {
            var query = _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId);

            if (excludeTokenId.HasValue)
            {
                var exclude = excludeTokenId.Value;
                query = query.Where(rt => rt.Id != exclude);
            }

            return await query
                .OrderByDescending(rt => rt.ExpiresAt)
                .FirstOrDefaultAsync();
        }

    }
}