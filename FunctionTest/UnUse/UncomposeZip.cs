using System.Collections;
using Newtonsoft.Json;

namespace FunctionTest.UnUse;

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

public class AudioFile
{
    public string RemoteName { get; set; }
}

public class PPPProgram
{
    static string zipPath =
        "Q:/GameManager/GameData/Global/Star Rail/game_2.3.0_2.4.0_hdiff_LtSmsraRrcMxgDYI.zip"; // 压缩文件路径

    static string extractPath = "Q:/GameManager/GameData/Global/Star Rail/extracted"; // 解压目标路径
    static string basePath = "Q:/GameManager/GameData/Global/Star Rail";
    static string delFileVol = "deletefiles.txt";
    static string hdiffFileVol = "hdifffiles.txt";

    public static async Task UnMain(string[] args)
    {
        int speedLimitMBps = 40; // 速度限制，每秒30 MB
        await ExtractZipWithSpeedLimit(zipPath, extractPath, speedLimitMBps);
       // await DeleteFile(Path.Combine(extractPath, delFileVol), Path.Combine(extractPath, hdiffFileVol), basePath);
    }

    static async Task DeleteFile(string delPath, string hdiffPath, string basePath)
    {

        var delList = await File.ReadAllLinesAsync(delPath);
        var files = delList.ToList();
        var hdifList = await File.ReadAllLinesAsync(hdiffPath);
        foreach (var hdif in hdifList)
        {
            var hdiff = JsonConvert.DeserializeObject<AudioFile>(hdif);
            files.Add(hdiff.RemoteName);
        }


        foreach (var fullPath in files.Select(file => Path.Combine(basePath, file)))
        {
            File.Delete(fullPath);
        }
    }

    static async Task ExtractZipWithSpeedLimit(string zipPath, string extractPath, int speedLimitMBps)
    {
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string destinationPath = Path.Combine(extractPath, entry.FullName);

                if (entry.FullName.EndsWith($"/"))
                {
                    // 创建目录
                    Directory.CreateDirectory(destinationPath);
                }
                else
                {
                    // 解压文件
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                    using (Stream fileStream = entry.Open())
                    using (FileStream outputStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                    {
                        await CopyStreamWithSpeedLimit(fileStream, outputStream, speedLimitMBps);
                    }
                }
            }
        }
    }

    static async Task CopyStreamWithSpeedLimit(Stream input, Stream output, int speedLimitMBps)
    {
        const int bufferSize = 16380; // 缓冲区大小
        byte[] buffer = new byte[bufferSize];
        int bytesRead;
        int speedLimitBytesPerSecond = speedLimitMBps * 1024 * 1024;
        DateTime startTime = DateTime.UtcNow;
        long totalBytesRead = 0;

        while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await output.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;

            TimeSpan elapsedTime = DateTime.UtcNow - startTime;
            double elapsedSeconds = elapsedTime.TotalSeconds;

            if (elapsedSeconds > 0)
            {
                double currentSpeed = totalBytesRead / elapsedSeconds;
                if (currentSpeed > speedLimitBytesPerSecond)
                {
                    double excessBytes = currentSpeed - speedLimitBytesPerSecond;
                    double excessTime = excessBytes / speedLimitBytesPerSecond;
                    int delay = (int)(excessTime * 1000); // 转换为毫秒

                    await Task.Delay(delay);
                }
            }
        }
    }
}