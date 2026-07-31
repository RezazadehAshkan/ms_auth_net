using AuthenticationService.Application.Interfaces;
using StackExchange.Redis;
namespace AuthenticationService.Infrastructure.Services;

public class DragonflyStore : ITemporaryStore
{
    private readonly IDatabase _db;

    public DragonflyStore(
        IConnectionMultiplexer connection)
    {
        _db = connection.GetDatabase();
    }
    public async Task<bool> SetAsync(string key, string value, TimeSpan expiration)
    {
        return await _db.StringSetAsync(key, value, expiration);
    }
    public async Task<string?> GetAsync(string key)
    {
        var val = await _db.StringGetAsync(key);
        return val.HasValue ? val.ToString() : null;
    }
    public async Task<bool> RemoveAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }
    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }
}