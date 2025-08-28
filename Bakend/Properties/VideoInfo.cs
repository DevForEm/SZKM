using Newtonsoft.Json;

public class AnimateInfo
{
    ///<summary>文件名</summary>
    public string? fileName { get; set; }

    /// <summary>文件前16MB的hash值</summary>
    public string? fileHash { get; set; }

    /// <summary>文件大小</summary>
    public string? fileSize { get; set; }

    /// <summary>视频时长</summary>
    public string? videoDuration { get; set; }

    /// <summary>匹配模式 </summary>
    public string? matchMode { get; set; }
}

public class BatchMatchResponse
{
    public List<BatchMatchResponseItem> items { get; set; }
    public int errorCode { get; set; }
    public bool success { get; set; }
    public string errorMessage { get; set; }
}

public class BatchMatchResponseItem
{
    public bool success { get; set; }
    public string fileHash { get; set; }
    public List<MatchResultV2> matchResult { get; set; }
}

public class MatchResultV2
{
    /// <summary> 弹幕库ID</summary>
    [JsonProperty(nameof(episodeId))]
    public int episodeId { get; set; }

    /// <summary> 作品ID </summary>
    [JsonProperty(nameof(animeId))]
    public int animeId { get; set; }

    /// <summary>作品标题 </summary>
    public string animeTitle { get; set; }

    /// <summary>剧集标题 </summary>
    public string episodeTitle { get; set; }

    /// <summary>作品类别</summary>
    public AnimationType type { get; set; }

    /// <summary>类型描述 </summary>
    public string typeDescription { get; set; }

    /// <summary>弹幕偏移时间值为负数，表示弹幕应提前多少秒出现</summary>
    public float shift { get; set; }
}
