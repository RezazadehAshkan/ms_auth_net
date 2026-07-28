using System;
using System.Threading.Tasks;
using AuthenticationService.Domain.Entities;

namespace AuthenticationService.Application.Interfaces
{
    public interface IRefreshTokenFactory
    {
        // Returns the created (hashed) RefreshToken entity and the unhashed token string for sending to the client
        Task<(RefreshToken? refToken, string? unHashedToken)> CreateAsync(Guid userId, RefreshToken? latestRefreshToken = null);

        Task<bool> UpdateOldRefreshTokenAsync(RefreshToken newRefreshToken, RefreshToken? latestRefreshToken = null);
    }
}