using Npgsql;

namespace Service.DabaseService;

public class PostgresDbService : IDataBaseService
{
    private readonly string _connectionString;


    public PostgresDbService(string connectionString)
    {
        _connectionString = connectionString;
        EnsureTableExists();
    }


    private void EnsureTableExists()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        string createTableSql =
            """
            CREATE  TABLE IF NOT EXISTS match_results (
            file_hash VARCHAR(255) PRIMARY KEY,
            episode_id INT NOT NULL,
            anim_id int NOT NULL ,
            anim_title VARCHAR(255) NOT NULL,
            episode_title VARCHAR(255) NOT NULL,
            type INT NOT NULL,
            type_description VARCHAR(255) NOT NULL,
            shift INT NOT NULL
            );   

            """;
        using var command = new NpgsqlCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    public void SaveMatchResult(string fileHash, MatchResultV2 matchResult)
    {
        var sql =
            """
            INSERT INTO match_results (file_hash, episode_id, anime_id, anime_title, episode_title, type, type_description, shift) 
            VALUES (@fileHash, @episodeId, @animeId, @animeTitle, @episodeTitle, @type, @typeDescription, @shift)
            ON CONFLICT (file_hash) DO UPDATE SET
            episode_id = @episodeId,
            anime_id = @animeId,
            anime_title = @animeTitle,
            episode_title = @episodeTitle,
            type = @type,
            type_description = @typeDescription,
            shift = @shift;
            """;
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@fileHash", fileHash);
        command.Parameters.AddWithValue("@episodeId", matchResult.episodeId);
        command.Parameters.AddWithValue("@animeId", matchResult.animeId);
        command.Parameters.AddWithValue("@animeTitle", matchResult.animeTitle);
        command.Parameters.AddWithValue("@episodeTitle", matchResult.episodeTitle);
        command.Parameters.AddWithValue("@type", matchResult.type);
        command.Parameters.AddWithValue("@typeDescription", matchResult.typeDescription);
        command.Parameters.AddWithValue("@typeDescription", matchResult.typeDescription);
        command.Parameters.AddWithValue("@shift", matchResult.shift);
        command.ExecuteNonQuery();
    }

    public MatchResultV2? GetMatchResult(string fileHash)
    {
        var sql =
            "SELECT * FROM match_results WHERE file_hash = @fileHash LIMIT 1";
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@fileHash", fileHash);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new MatchResultV2
            {
                animeId = reader.GetInt32(reader.GetOrdinal("anime_id")),
                animeTitle = reader.GetString(reader.GetOrdinal("anime_title")),
                episodeId = reader.GetInt32(reader.GetOrdinal("episode_id")),
                episodeTitle = reader.GetString(reader.GetOrdinal("episode_title")),
                typeDescription = reader.GetString(reader.GetOrdinal("type_description")),
                type = (AnimationType)reader.GetInt32(reader.GetOrdinal("type_description")),
                shift = (float)reader.GetDouble(reader.GetOrdinal("shift"))
            };
        }
        return null;
    }
}