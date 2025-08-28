namespace EventModule;

public interface IEvent
{
}

public interface IEventListener
{
}

public interface IEventListener<in T> : IEventListener where T : IEvent
{
    void OnEvent(T evt, params object[] args);
}

public interface IEventListenerAsync<in T> : IEventListener where T : IEvent
{
    Task OnEvent(T evt, params object[] args);
}