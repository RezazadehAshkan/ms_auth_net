namespace AuthenticationService.Application.Users.ForgotPassword
{
    public record ResetPasswordRequest(string ResetToken, string NewPassword);
}