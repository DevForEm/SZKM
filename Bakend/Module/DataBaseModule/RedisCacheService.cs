using StackExchange.Redis;

namespace Service.DabaseService;

public class RedisCacheService
{
    private readonly ConnectionMultiplexer _redis;
    private readonly bool _isConnected;

    public RedisCacheService(string connectionString)
    {
        try
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _isConnected = true;
        }
        catch (RedisConnectionException)
        {
            _isConnected = false;
            Logger.Log($"Redis using Status: {_isConnected}");
        }
    }

    public bool IsAvailable => _isConnected;

    public void Set(string key, string value, TimeSpan? expiry = null)
    {
        if (!IsAvailable) return;
        var db = _redis.GetDatabase();
        db.StringSet(key, value, expiry);
    }

    public string? Get(string key)
    {
        if (!IsAvailable) return null;
        var db = _redis.GetDatabase();
        return db.StringGet(key);
    }

}