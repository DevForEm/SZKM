using System;
using System.IO;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public static class Logger
{
    public static LogLevel CurrentLogLevel = LogLevel.Debug;

    public static bool EnableFileOutput = false;
    private static string _logFilePath = "app.log";

    public static void SetLogPath(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public static void Log(object msg, string tag = "Default")
    {
        WriteLog(LogLevel.Debug, msg, tag);
    }

    public static void LogInfo(object msg, string tag = "Default")
    {
        WriteLog(LogLevel.Info, msg, tag);
    }

    public static void LogWarning(object msg, string tag = "Default")
    {
        WriteLog(LogLevel.Warning, msg, tag);
    }

    public static void LogError(object msg, string tag = "Default")
    {
        WriteLog(LogLevel.Error, msg, tag);
    }

    private static void WriteLog(LogLevel level, object msg, string tag)
    {
        if (level < CurrentLogLevel) return;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string log = $"[{timestamp}] [{level}] [{tag}] {msg}";

        // 控制台输出（带颜色）
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = GetColor(level);
        Console.WriteLine(log);
        Console.ForegroundColor = originalColor;

        // 文件输出
        if (EnableFileOutput)
        {
            try
            {
                File.AppendAllText(_logFilePath, log + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Logger] Failed to write log file: {e.Message}");
                Console.ForegroundColor = originalColor;
            }
        }
    }

    private static ConsoleColor GetColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
    }
}