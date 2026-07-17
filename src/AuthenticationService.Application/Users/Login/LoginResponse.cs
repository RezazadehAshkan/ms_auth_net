
namespace AuthenticationService.Application.Users.Login
{
    public record LoginResponse(string AccessToken, string RefreshToken, long ExpiresIn);
}