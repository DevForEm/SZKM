namespace SZKMu.Properties;

public class DownloadGameResource
{
    public DownloadPackageState Game { get; set; }
    public List<DownloadPackageState> Voices { get; set; } = new(4);
    public long FreeSpace { get; set; }
}

public class DownloadPackageState
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string PackageSize { get; set; }
    public string DecompressedSize { get; set; }
    public string DownloadSpeed { get; set; }
}