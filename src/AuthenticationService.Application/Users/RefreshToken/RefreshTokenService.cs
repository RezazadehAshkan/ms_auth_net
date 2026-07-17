using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;

namespace AuthenticationService.Application.Users.RefreshToken
{

    public class RefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ITokenService _tokenService;
        public RefreshTokenService(IRefreshTokenRepository refreshTokens, ITokenService tokenService)
        {
            _refreshTokens = refreshTokens;
            _tokenService = tokenService;
        }
        public async Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            string hashedToken = await _tokenService.HashTokenAsync(request.RefreshToken);
            var oldRefreshToken = await _refreshTokens.GetRefreshTokenAsync(hashedToken);
            if (oldRefreshToken == null)
            {
                return null;
            }
            if (!oldRefreshToken.IsActive)
            {
                return null;
            }
            var newAccessToken = _tokenService.GenerateJwtToken(oldRefreshToken.UserId.ToString(), oldRefreshToken.User.Username, oldRefreshToken.User.Email);
            var unHashedNewRefreshToken = _tokenService.GenerateRefreshToken();
            var hashedNewRefreshToken = await _tokenService.HashTokenAsync(unHashedNewRefreshToken);
            var newRefreshToken = new AuthenticationService.Domain.Entities.RefreshToken(
                userId: oldRefreshToken.UserId,
                hashedToken: hashedNewRefreshToken,
                expiresAt: DateTime.UtcNow.AddDays(7)
                            );

            await _refreshTokens.AddRefreshToken(newRefreshToken);
            oldRefreshToken.ReplaceWith(newRefreshToken.Id);
            _refreshTokens.UpdateRefreshTokenAsync(oldRefreshToken);
            return new RefreshTokenResponse(newAccessToken, unHashedNewRefreshToken);
        }
    }
}