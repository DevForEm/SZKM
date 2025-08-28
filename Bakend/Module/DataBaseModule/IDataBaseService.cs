namespace Service.DabaseService;

public interface IDataBaseService
{

    /// <summary>
    /// 保存视频匹配结果到数据库。
    /// </summary>
    /// <param name="fileHash">视频文件的哈希值。</param>
    /// <param name="matchResult">匹配结果数据。</param>
    void SaveMatchResult(string fileHash, MatchResultV2 matchResult);

    /// <summary>
    /// 从数据库获取视频匹配结果。
    /// </summary>
    /// <param name="fileHash">视频文件的哈希值。</param>
    /// <returns>匹配结果，如果未找到则返回 null。</returns>
    MatchResultV2? GetMatchResult(string fileHash);
}