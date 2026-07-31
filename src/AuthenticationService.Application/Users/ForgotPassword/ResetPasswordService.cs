using AuthenticationService.Application.Interfaces;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Domain.Repositories;

namespace AuthenticationService.Application.Users.ForgotPassword
{

    public class ResetPasswordService
    {
        private readonly IUserRepository _users;
        private readonly ITemporaryStore _temporaryStore;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private string generateResetTokenTempDBKey(string resetToken)
        {
            return $"ResetTokens:{resetToken}";
        }

        public ResetPasswordService(IUserRepository users, ITemporaryStore temporaryStore, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _users = users;
            _temporaryStore = temporaryStore;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<ForgotPasswordResponse> RequestOTPAsync(ForgotPasswordRequest request)
        {
            // I should add logics for duplicate requests later
            var userExists = await _users.UserExistsByEmailAsync(request.EmailAddress);
            if (!userExists)
            {
                return new ForgotPasswordResponse(false, "User with the provided email does not exist.");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var setOTPResult = await _temporaryStore.SetAsync(request.EmailAddress, otp, TimeSpan.FromMinutes(5));
            if (setOTPResult)
            {
                // Here you would typically send the OTP to the user's email.
                return new ForgotPasswordResponse
                (
                    true,
                    "OTP has been sent to your email."
                );
            }
            else
            {
                return new ForgotPasswordResponse
                (
                    false,
                    "Failed to generate OTP. Please try again."
                );
            }

        }
        public async Task<VerifyOtpResponse?> VerifyOtpAsync(VerifyOtpRequest request)
        {
            // I should add logics for duplicate requests later
            var tempCode = await _temporaryStore.GetAsync(request.EmailAddress);
            if (tempCode == null)
            {
                return null;
            }
            int.TryParse(tempCode, out int convertedTempCode);
            if (convertedTempCode == request.OTP)
            {
                var userId = await _users.GetUserIdByEmailAsync(request.EmailAddress);
                if (userId == null)
                {
                    return null;
                }
                var resetToken = _tokenService.GenerateRandomToken(16);
                var setResetTokenResult = await _temporaryStore.SetAsync(generateResetTokenTempDBKey(resetToken), userId, TimeSpan.FromMinutes(5));
                _temporaryStore.RemoveAsync(request.EmailAddress);
                return new VerifyOtpResponse(resetToken);
            }
            else
            {
                return null;
            }
        }
        public async Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var userId = await _temporaryStore.GetAsync(generateResetTokenTempDBKey(request.ResetToken));
            if (userId == null)
            {
                return null;
            }
            var user = await _users.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            user.UpdatePassword(_passwordHasher.HashPassword(request.NewPassword));
            await _users.UpdateUserAsync(user);
            //I should make the formula of resettoken key accessible by each consumer
            _temporaryStore.RemoveAsync($"ResetTokens:{request.ResetToken}");
            return new ResetPasswordResponse("success");
        }
    }
}