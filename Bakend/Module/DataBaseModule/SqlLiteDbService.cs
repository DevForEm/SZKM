using System.Data.SQLite;

namespace Service.DabaseService;

public class SqlLiteDbService : IDataBaseService
{

    private readonly string _connectionString;

    public SqlLiteDbService(string connectionString)
    {
        _connectionString = connectionString;
        EnsureTableExists();
    }

    private void EnsureTableExists()
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        string createTableSql = @"
            CREATE TABLE IF NOT EXISTS match_results (
                file_hash TEXT PRIMARY KEY,
                episode_id INTEGER NOT NULL,
                anime_id INTEGER NOT NULL,
                anime_title TEXT NOT NULL,
                episode_title TEXT NOT NULL,
                type INTEGER NOT NULL,
                type_description TEXT NOT NULL,
                shift REAL NOT NULL
            );";

        using var command = new SQLiteCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    public void SaveMatchResult(string fileHash, MatchResultV2 matchResult)
    {
        string sql = @"
            INSERT INTO match_results (file_hash, episode_id, anime_id, anime_title, episode_title, type, type_description, shift) 
            VALUES (@fileHash, @episodeId, @animeId, @animeTitle, @episodeTitle, @type, @typeDescription, @shift)
            ON CONFLICT (file_hash) DO UPDATE SET
            episode_id = excluded.episode_id,
            anime_id = excluded.anime_id,
            anime_title = excluded.anime_title,
            episode_title = excluded.episode_title,
            type = excluded.type,
            type_description = excluded.type_description,
            shift = excluded.shift;";

        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        using var cmd = new SQLiteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@fileHash", fileHash);
        cmd.Parameters.AddWithValue("@episodeId", matchResult.episodeId);
        cmd.Parameters.AddWithValue("@animeId", matchResult.animeId);
        cmd.Parameters.AddWithValue("@animeTitle", matchResult.animeTitle);
        cmd.Parameters.AddWithValue("@episodeTitle", matchResult.episodeTitle);
        cmd.Parameters.AddWithValue("@type", (int)matchResult.type);
        cmd.Parameters.AddWithValue("@typeDescription", matchResult.typeDescription);
        cmd.Parameters.AddWithValue("@shift", matchResult.shift);

        cmd.ExecuteNonQuery();
    }

    public MatchResultV2? GetMatchResult(string fileHash)
    {
        string sql = "SELECT * FROM match_results WHERE file_hash = @fileHash LIMIT 1";

        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();
        using var cmd = new SQLiteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@fileHash", fileHash);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new MatchResultV2
            {
                episodeId = reader.GetInt32(reader.GetOrdinal("episode_id")),
                animeId = reader.GetInt32(reader.GetOrdinal("anime_id")),
                animeTitle = reader.GetString(reader.GetOrdinal("anime_title")),
                episodeTitle = reader.GetString(reader.GetOrdinal("episode_title")),
                type = (AnimationType)reader.GetInt32(reader.GetOrdinal("type")),
                typeDescription = reader.GetString(reader.GetOrdinal("type_description")),
                shift = (float)reader.GetDouble(reader.GetOrdinal("shift"))
            };
        }
        return null;
    }
}