using Module;
using Newtonsoft.Json;

namespace ConfigManager;

public class ConfigModule : BaseModule
{
    private const string ConfigFileName = "config.json";
    private static readonly object Locker = new();
    private AppSettings? _settings;
    private string _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
    private static readonly AppSettings DefaultSettings = new();

    public static ConfigModule Instance => ModuleCenter.SGetModule<ConfigModule>();

    public static AppSettings Setting
    {
        get
        {
            lock (Locker)
            {
                if (Instance._settings == null)
                {
                    Instance.Load();
                }
                return Instance._settings;
            }
        }
    }

    protected override void OnPreInit()
    {
        base.OnPreInit();
        _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        //_configFilePath = ConfigFileName;
        Logger.LogInfo($"Configuration file: {AppDomain.CurrentDomain.BaseDirectory}");
        Load();
    }


    public void Load()
    {
        lock (Locker)
        {
            _settings = new AppSettings();
            if (File.Exists(_configFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_configFilePath);
                    var sets = JsonConvert.DeserializeObject<AppSettings>(json);

                    foreach (var property in typeof(AppSettings).GetProperties())
                    {
                        var value = property.GetValue(sets);
                        if (value != null)
                        {
                            property.SetValue(_settings, value);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error loading configuration: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Logger.LogInfo("Configuration file not found. Using default settings.");
                Save();
            }
        }
    }

    public void Save()
    {
        lock (Locker)
        {
            try
            {
                var toSave = new Dictionary<string, object>();

                foreach (var prop in typeof(AppSettings).GetProperties())
                {
                    var currentValue = prop.GetValue(_settings);
                    var defaultValue = prop.GetValue(DefaultSettings);

                    if (object.Equals(currentValue, defaultValue)) continue;
                    if (currentValue != null) toSave.Add(prop.Name, currentValue);
                }
                var json = JsonConvert.SerializeObject(toSave, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);
                Logger.LogInfo($"Configuration saved: {_configFilePath}");

            }
            catch (Exception e)
            {
                Logger.LogError($"Error saving configuration: {e.Message}");
            }
        }
    }
}