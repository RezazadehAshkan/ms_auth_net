using System;
using AuthenticationService.Application.Interfaces;
namespace AuthenticationService.Infrastructure.Services
{
    public sealed class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly int _tokenExpirationMs;

        public TokenService(string secretKey, int tokenExpirationMs)
        {
            _secretKey = secretKey;
            _tokenExpirationMs = tokenExpirationMs;
        }

        public string GenerateJwtToken(string userId, string username, string email)
        {
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
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateJwtToken(string token, out string userId, out string username, out string email)
        {
            userId = string.Empty;
            username = string.Empty;
            email = string.Empty;

            if (string.IsNullOrEmpty(token))
                return false;

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

                return true;
            }
            catch
            {
                return false;
            }
        }
        private string HashTokenSync(string token)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(token);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        public async Task<string> HashTokenAsync(string token)
        {
            return await Task.Run(() => HashTokenSync(token));
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}