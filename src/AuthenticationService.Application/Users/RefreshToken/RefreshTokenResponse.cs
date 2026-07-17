namespace AuthenticationService.Application.Users.RefreshToken
{
    public record RefreshTokenResponse(string AccessToken, string RefreshToken);
}