namespace AuthenticationService.Application.Users.ForgotPassword
{
    public record VerifyOtpRequest(string EmailAddress, int OTP);
}