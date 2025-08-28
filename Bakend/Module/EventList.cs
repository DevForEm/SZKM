namespace EventModule;

public abstract class EventList
{
    public abstract class EventHttpServerLaunch : IEvent
    {
        public string Url { get; set; }
        public int Port { get; set; } = 0;


        public EventHttpServerLaunch(int port, string url = "*")
        {
            Url = url;
            Port = port;

        }
    }

    public class EventHttpServerListener(string path, Action onRequest) : IEvent
    {
        public string Path { get; set; } = path;
        public event Action OnRequest = onRequest;

    }


}