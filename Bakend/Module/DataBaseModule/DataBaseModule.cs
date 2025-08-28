using Module;
using Npgsql;
using Service.DabaseService;
using ConfigManager;

namespace Bakend.Module.DataBaseModule;

public class DataBaseModule : BaseModule
{

    public IDataBaseService DbService { get; private set; }
    public RedisCacheService CacheService { get; private set; }


    protected override void OnPostInit()
    {
        base.OnPostInit();
        var host = ConfigModule.Setting.PostgresDbHost;
        var port = ConfigModule.Setting.PostgresDbPort;
        var user = ConfigModule.Setting.PostgresDbUser;
        var password = ConfigModule.Setting.PostgresDbPassword;
        var db = ConfigModule.Setting.PostgresDbDatabase;

        string postgresConnStr = $"Host={host};Username={user};Password={password};Database={db};Port={port}";
        string sqliteConnStr = $"Data Source={ConfigModule.Setting.SqLiteDbPath}";
        string redisConnStr =
            $"{ConfigModule.Setting.RedisHost}:{ConfigModule.Setting.RedisPort}," +
            $"password={ConfigModule.Setting.RedisPassword}";

        try
        {
            using var pgConnection = new NpgsqlConnection(postgresConnStr);
            pgConnection.Open();
            DbService = new PostgresDbService(postgresConnStr);
            Logger.Log("DatabaseModule: PostgreSQL 连接成功");
        }
        catch (NpgsqlException)
        {
            DbService = new SqlLiteDbService(sqliteConnStr);
            Logger.Log("DatabaseModule: PostgreSQL 连接失败，切换到 SQLite。");
        }
        Logger.Log($"{redisConnStr}");
        CacheService = new RedisCacheService(redisConnStr);
        if (CacheService.IsAvailable)
        {
            Logger.Log($"{TAG}: Redis 缓存已连接。");
        }
        else
        {
            Logger.Log($"{TAG}: Redis 缓存连接失败。");
        }

    }


}