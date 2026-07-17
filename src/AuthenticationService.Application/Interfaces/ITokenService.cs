namespace AuthenticationService.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(string userId, string username, string email);
        bool ValidateJwtToken(string token, out string userId, out string username, out string email);
    
    }
}