namespace ConfigManager;

public class AppSettings
{
    #region MainProgress

    /// <summary>
    /// 程序监听端口
    /// </summary>
    public int Port { get; set; } = 7800;

    public LogLevel LogLevel { get; set; } = LogLevel.Debug;

    #endregion
    #region SQLite

    /// <summary>
    /// SQLite路径
    /// </summary>
    public string SqLiteDbPath { get; set; } = "sqlite.db";

    #endregion

    #region Postgre

    /// <summary>
    /// PostreDb主机地址
    /// </summary>
    public string PostgresDbHost { get; set; } = "127.0.0.1";

    /// <summary>
    /// POstgre端口
    /// </summary>
    public string PostgresDbPort { get; set; } = "5432";

    /// <summary>
    /// Postgre用户
    /// </summary>
    public string PostgresDbUser { get; set; } = "postgres";

    /// <summary>
    /// Postgre密码
    /// </summary>
    public string PostgresDbPassword { get; set; } = "mypassword";

    /// <summary>
    /// Postgre数据库
    /// </summary>
    public string PostgresDbDatabase { get; set; } = "mydb";

    #endregion
    #region Redis

    public string RedisHost { get; set; } = "localhost";
    public string RedisPort { get; set; } = "6379";
    public string RedisPassword { get; set; } = "mypassword";

    #endregion


}