using AuthenticationService.Application.Interfaces;

namespace AuthenticationService.Infrastructure.Services
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
