using AuthenticationService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Infrastructure.Services
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;
        private readonly ILogger<PasswordHasher> _logger;

        public PasswordHasher(ILogger<PasswordHasher> logger) => _logger = logger;

        public string HashPassword(string password)
        {
            _logger.LogInformation("Hashing password");
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(password);
                var hash = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
                _logger.LogInformation("Password hashing completed");
                return hash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password hashing failed");
                throw new InvalidOperationException("Password hashing failed.", ex);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            _logger.LogInformation("Verifying password");
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(password);
                ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);
                var valid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                _logger.LogInformation("Password verification completed with result {IsValid}", valid);
                return valid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password verification failed");
                throw new InvalidOperationException("Password verification failed.", ex);
            }
        }
    }
}
