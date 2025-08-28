namespace Module;

public interface IModule
{
    string TAG { get; }
    bool IsInitialized { get; }
    void PreInitialize();
    void Initialize();
    void PostInitialize();
    void UnInitialize();
}

public abstract class BaseModule : IModule
{
    public virtual string TAG => GetType().Name;

    public bool IsInitialized { get; protected set; }

    public void PreInitialize()
    {
        OnPreInit();
    }

    public void Initialize()
    {
        OnInit();
    }

    public void PostInitialize()
    {
        OnPostInit();
    }

    public void UnInitialize()
    {
        UnInit();
    }


    protected virtual void OnPreInit()
    {
    }

    protected virtual void OnPostInit()
    {
        IsInitialized = true;
    }

    protected virtual void OnInit()
    {
    }

    protected virtual void UnInit()
    {
    }
}