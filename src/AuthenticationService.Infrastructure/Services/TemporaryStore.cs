using AuthenticationService.Application.Interfaces;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
namespace AuthenticationService.Infrastructure.Services;

public class DragonflyStore : ITemporaryStore
{
    private readonly IDatabase _db;
    private readonly ILogger<DragonflyStore> _logger;

    public DragonflyStore(
        IConnectionMultiplexer connection, ILogger<DragonflyStore> logger)
    {
        _db = connection.GetDatabase();
        _logger = logger;
    }
    public async Task<bool> SetAsync(string key, string value, TimeSpan expiration)
    {
        _logger.LogInformation("Setting temporary-store value for key {Key}", key);
        try { return await _db.StringSetAsync(key, value, expiration); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to set temporary-store value for key {Key}", key); throw new InvalidOperationException("Failed to set temporary-store value.", ex); }
    }
    public async Task<string?> GetAsync(string key)
    {
        _logger.LogInformation("Getting temporary-store value for key {Key}", key);
        try { var val = await _db.StringGetAsync(key); return val.HasValue ? val.ToString() : null; }
        catch (Exception ex) { _logger.LogError(ex, "Failed to get temporary-store value for key {Key}", key); throw new InvalidOperationException("Failed to get temporary-store value.", ex); }
    }
    public async Task<bool> RemoveAsync(string key)
    {
        _logger.LogInformation("Removing temporary-store value for key {Key}", key);
        try { return await _db.KeyDeleteAsync(key); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to remove temporary-store value for key {Key}", key); throw new InvalidOperationException("Failed to remove temporary-store value.", ex); }
    }
    public async Task<bool> KeyExistsAsync(string key)
    {
        _logger.LogInformation("Checking temporary-store key {Key}", key);
        try { return await _db.KeyExistsAsync(key); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to check temporary-store key {Key}", key); throw new InvalidOperationException("Failed to check temporary-store key.", ex); }
    }
}