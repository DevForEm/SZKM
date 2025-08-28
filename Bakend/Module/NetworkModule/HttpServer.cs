using System.Net;
using EventModule;

namespace Module.NetworkModule;

public class HttpServer :
    IEventListener<EventList.EventHttpServerLaunch>,
    IEventListener<EventList.EventHttpServerListener>
{
    private readonly HttpListener _listener = new();

    private int _port = 0;
    private string _url = "";

    private void Launch()
    {
        _listener.Prefixes.Add($"http://{_url}:{_port}/");
        _listener.Prefixes.Add($"https://{_url}:{_port}/");

        _listener.Start();
    }

    private void RegisterListener(string path,Action onRequest)
    {
        //_listener.

    }
    private void Dispose()
    {
        _listener.Close();
    }

    private void Stop()
    {
        _listener.Stop();
    }

    public void OnEvent(EventList.EventHttpServerLaunch evt, params object[] args)
    {
        _port = evt.Port;
        _url = evt.Url;
        Launch();
    }

    public void OnEvent(EventList.EventHttpServerListener evt, params object[] args)
    {
    }
}