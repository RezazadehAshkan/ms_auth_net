namespace AuthenticationService.Application.Interfaces;

public interface ITemporaryStore
{
    Task<bool> SetAsync(string key, string value, TimeSpan expiration);
    Task<string?> GetAsync(string key);
    Task<bool> RemoveAsync(string key);
    Task<bool> KeyExistsAsync(string key);
}