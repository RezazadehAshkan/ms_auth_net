using System;
using AuthenticationService.Domain.Entities;

namespace AuthenticationService.Domain.Repositories
{
    public interface IRefreshTokenRepository
    {
        // Define methods for refresh token repository operations
        Task<Guid> AddRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string hashedRefreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetLastRefreshTokenByUserIdAsync(Guid userId, Guid? excludeTokenId = null);
    }
}