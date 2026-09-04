using System.Security.Cryptography;
using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Application.Users.ForgotPassword
{

    public class ResetPasswordService
    {
        private readonly IUserRepository _users;
        private readonly ITemporaryStore _temporaryStore;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly ILogger<ResetPasswordService> _logger;
        private string generateResetTokenTempDBKey(string resetToken)
        {
            _logger.LogInformation("Generating temporary reset-token key");
            return $"ResetTokens:{resetToken}";
        }

        private string GenerateSecureOtp(int digits = 6)
        {
            _logger.LogInformation("Generating cryptographically secure {Digits}-digit OTP", digits);
            using (var rng = RandomNumberGenerator.Create())
            {
                // 4 bytes = 32 bits of entropy, far more than a 6-digit OTP needs
                // (max 999_999 < 2^20 ≈ 1M).
                var buffer = new byte[4];
                rng.GetBytes(buffer);

                // Map [0, 2^32) -> [0, 10^digits). Tiny modulo bias is acceptable for OTPs.
                var raw = BitConverter.ToUInt32(buffer, 0);
                var max = (uint)Math.Pow(10, digits);   // 1_000_000 for 6 digits
                var otp = raw % max;
                return otp.ToString($"D{digits}");      // zero-pad so it's always 6 chars
            }
        }

        public ResetPasswordService(IUserRepository users, ITemporaryStore temporaryStore, IPasswordHasher passwordHasher, ITokenService tokenService, ILogger<ResetPasswordService> logger)
        {
            _users = users;
            _temporaryStore = temporaryStore;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<ForgotPasswordResponse> RequestOTPAsync(ForgotPasswordRequest request)
        {
            _logger.LogInformation("Starting OTP request for email {Email}", request.EmailAddress);
            // I should add logics for duplicate requests later
            var userExists = await _users.UserExistsByEmailAsync(request.EmailAddress);
            if (!userExists)
            {
                _logger.LogInformation("OTP request rejected because the email does not exist");
                return new ForgotPasswordResponse(false, "User with the provided email does not exist.");
            }

            var otp = GenerateSecureOtp(digits: 6);
            var setOTPResult = await _temporaryStore.SetAsync(request.EmailAddress, otp, TimeSpan.FromMinutes(5));
            if (setOTPResult)
            {
                _logger.LogInformation("OTP stored successfully for email {Email}", request.EmailAddress);
                // Here you would typically send the OTP to the user's email.
                return new ForgotPasswordResponse
                (
                    true,
                    "OTP has been sent to your email."
                );
            }
            else
            {
                _logger.LogError("Failed to store OTP for email {Email}", request.EmailAddress);
                return new ForgotPasswordResponse
                (
                    false,
                    "Failed to generate OTP. Please try again."
                );
            }

        }
        public async Task<VerifyOtpResponse?> VerifyOtpAsync(VerifyOtpRequest request)
        {
            _logger.LogInformation("Starting OTP verification for email {Email}", request.EmailAddress);
            // I should add logics for duplicate requests later
            var tempCode = await _temporaryStore.GetAsync(request.EmailAddress);
            if (tempCode == null)
            {
                _logger.LogInformation("OTP verification failed because no OTP was found");
                return null;
            }
            int.TryParse(tempCode, out int convertedTempCode);
            if (convertedTempCode == request.OTP)
            {
                var userId = await _users.GetUserIdByEmailAsync(request.EmailAddress);
                if (userId == null)
                {
                    _logger.LogInformation("OTP verification failed because no user ID was found");
                    return null;
                }
                var resetToken = _tokenService.GenerateRandomToken(16);
                await _temporaryStore.SetAsync(generateResetTokenTempDBKey(resetToken), userId, TimeSpan.FromMinutes(5));
                _temporaryStore.RemoveAsync(request.EmailAddress);
                _logger.LogInformation("OTP verification completed for email {Email}", request.EmailAddress);
                return new VerifyOtpResponse(resetToken);
            }
            else
            {
                _logger.LogInformation("OTP verification failed because the OTP was invalid");
                return null;
            }
        }
        public async Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordRequest request)
        {
            _logger.LogInformation("Starting password reset");
            var userId = await _temporaryStore.GetAsync(generateResetTokenTempDBKey(request.ResetToken));
            if (userId == null)
            {
                _logger.LogInformation("Password reset failed because the reset token was not found");
                return null;
            }
            var user = await _users.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogInformation("Password reset failed because the user was not found");
                return null;
            }
            user.UpdatePassword(_passwordHasher.HashPassword(request.NewPassword));
            await _users.UpdateUserAsync(user);
            //I should make the formula of resettoken key accessible by each consumer
            _temporaryStore.RemoveAsync($"ResetTokens:{request.ResetToken}");
            _logger.LogInformation("Password reset completed for user ID {UserId}", userId);
            return new ResetPasswordResponse("success");
        }
    }
}