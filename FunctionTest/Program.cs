using System.Collections.Concurrent;
using Bakend.Module.DataBaseModule;
using ConfigManager;
using EventModule;
using Module;

namespace FunctionTest;

public static class Program
{
    public static void Main(string[] args)
    {
        //ModuleCenter.SCreateModule<ConfigModule>();
        ModuleCenter.InitModules<ConfigModule>();

        Logger.Log($"{ConfigModule.Setting.Port}");
        Logger.Log($"{ConfigModule.Setting.PostgresDbPassword}");
        Logger.Log($"{ConfigModule.Setting.RedisPassword}");
        ModuleCenter.InitModules<DataBaseModule>();
        //  ConfigModule.Instance.Save();

    }


}
