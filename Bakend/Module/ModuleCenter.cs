namespace Module;

public class ModuleCenter
{
    private static ModuleCenter _instance;


    private readonly Dictionary<string, IModule?> _modules = new();

    static ModuleCenter()
    {
        _instance = new ModuleCenter();
    }

    private ModuleCenter()
    {
    }

    #region ---MOdule Functions---

    /// <summary>
    /// Creat and Register Module
    /// </summary>
    /// <typeparam name="T">Module Type</typeparam>
    private void CreateModule<T>() where T : class, IModule, new()
    {
        var moduleName = typeof(T).Name;
        if (_modules.ContainsKey(moduleName))
        {
            Logger.LogInfo($"module {moduleName} is already initialized");
        }

        var module = new T();
        _modules.Add(moduleName, module);
    }

    private void CreateModule(IModule? module)
    {
        if (module == null) return;

        var moduleName = module.GetType().Name;
        if (!_modules.TryAdd(moduleName, module))
        {
            Logger.LogInfo($"module {moduleName} is already initialized");
            return;
        }

        Logger.LogInfo($"module {moduleName} is initialized from instance");
    }

    /// <summary>
    /// Get Module
    /// </summary>
    /// <typeparam name="T">Module Type</typeparam>
    /// <returns></returns>
    private T? GetModule<T>() where T : class, IModule, new()
    {
        var key = typeof(T).Name;
        if (_modules.TryGetValue(key, out var module))
        {
            return module as T;
        }

        Logger.LogWarning($"module {key} is not initialized");
        return null;
    }

    /// <summary>
    /// Unregister Module
    /// </summary>
    /// <typeparam name="T">Module Type</typeparam>
    private void UnregisterModule<T>() where T : class, IModule, new()
    {
        var key = typeof(T).Name;
        if (!_modules.TryGetValue(key, out var module))
        {
            Logger.LogWarning($"module {key} is not initialized");
            return;
        }

        module?.UnInitialize();
        _modules.Remove(key);
        Logger.LogInfo($"module {key} is unregistered");
    }

    /// <summary>
    /// Unregister All Modules
    /// </summary>
    public void UnregisterAllModules()
    {
        foreach (var module in _modules)
        {
            module.Value?.UnInitialize();
        }

        _modules.Clear();
        Logger.LogInfo("All modules are unregistered");
    }

    #endregion


    #region ---Static Interface---

    public static ModuleCenter Instance => _instance ??= new ModuleCenter();

    public static void SCreateModule<T>() where T : class, IModule, new()
    {
        _instance.CreateModule<T>();
    }

    public static T SGetModule<T>() where T : class, IModule, new()
    {
        return _instance.GetModule<T>()!;
    }

    /// <summary>
    /// Init Module
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public static void InitModules<T>() where T : class, IModule, new()
    {
        _instance.CreateModule<T>();
        _instance.GetModule<T>()?.PreInitialize();
        _instance.GetModule<T>()?.Initialize();
        _instance.GetModule<T>()?.PostInitialize();
    }

    /// <summary>
    /// Init Modules 慎用
    /// </summary>
    /// <param name="modules">Modules to init</param>
    public static void InitModules(params IModule?[] modules)
    {
        foreach (var module in modules)
        {
            _instance.CreateModule(module);
        }

        foreach (var module in modules)
        {
            module?.PreInitialize();
            module?.Initialize();
            module?.PostInitialize();
        }
    }

    /// <summary>
    /// Unregister Module
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void UnregisterModules<T>() where T : class, IModule, new()
    {
        _instance.UnregisterModule<T>();
        Logger.LogInfo($"module {typeof(T).Name} is unregistered");
    }

    #endregion
}