using System.IO;
using SZKMu.Properties;
using Newtonsoft.Json;

namespace SZKMu.Tools;

public class CheckData
{
    private const string WorkPath = "Q:/GameManager/GameData/Native/Genshin Impact Game";
    List<DownloadPackageState> remote_Package = new();
    private List<DownloadPackageState> local_Package = new();
    private List<PKG_Version> localCache = new();

    public async void ReadRemoteData()
    {
        var remoteCache = await File.ReadAllLinesAsync($"{WorkPath}/pkg_version");

        foreach (var package in remoteCache)
        {
            var result = JsonConvert.DeserializeObject<PKG_Version>(package);
            if (result == null)
                return;
            localCache.Add(new PKG_Version
            {
                FilePath = result.FilePath,
                MD5 = result.MD5,
                Hash = result.Hash,
                FileSize = result.FileSize
            });
        }
    }


    public void VerifyData()
    {
    }
}