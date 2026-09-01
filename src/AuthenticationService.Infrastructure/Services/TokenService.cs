using System;
using AuthenticationService.Application.Interfaces;
using Microsoft.Extensions.Logging;
namespace AuthenticationService.Infrastructure.Services
{
    public sealed class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly int _tokenExpirationMs;
        private readonly ILogger<TokenService> _logger;

        public TokenService(string secretKey, int tokenExpirationMs, ILogger<TokenService> logger)
        {
            _secretKey = secretKey;
            _tokenExpirationMs = tokenExpirationMs;
            _logger = logger;
        }

        public string GenerateJwtToken(string userId, string username, string email)
        {
            _logger.LogInformation("Generating JWT for user ID {UserId}", userId);
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("id", userId),
                    new System.Security.Claims.Claim("username", username),
                    new System.Security.Claims.Claim("email", email)
                }),
                Expires = DateTime.UtcNow.AddMilliseconds(_tokenExpirationMs),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var result = tokenHandler.WriteToken(token);
            _logger.LogInformation("JWT generation completed for user ID {UserId}", userId);
            return result;
        }

        public bool ValidateJwtToken(string token, out string userId, out string username, out string email)
        {
            _logger.LogInformation("Validating JWT");
            userId = string.Empty;
            username = string.Empty;
            email = string.Empty;

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogInformation("JWT validation failed because the token was empty");
                return false;
            }

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = (System.IdentityModel.Tokens.Jwt.JwtSecurityToken)validatedToken;
                userId = jwtToken.Claims.First(x => x.Type == "id").Value;
                username = jwtToken.Claims.First(x => x.Type == "username").Value;
                email = jwtToken.Claims.First(x => x.Type == "email").Value;

                _logger.LogInformation("JWT validation completed for user ID {UserId}", userId);
                return true;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                _logger.LogError(ex, "JWT validation failed");
                return false;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "JWT validation failed");
                return false;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "JWT validation failed");
                return false;
            }
        }
        private string HashTokenSync(string token)
        {
            _logger.LogInformation("Hashing refresh token");
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(token);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        public async Task<string> HashTokenAsync(string token)
        {
            try
            {
                var result = await Task.Run(() => HashTokenSync(token));
                _logger.LogInformation("Refresh token hashing completed");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token hashing failed");
                throw;
            }
        }
        public string GenerateRefreshToken()
        {
            _logger.LogInformation("Generating refresh token");
            return GenerateRandomToken();
        }
        public string GenerateRandomToken(int bytes = 32)
        {
            _logger.LogInformation("Generating random token with {ByteCount} bytes", bytes);
            var randomNumber = new byte[bytes];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}