using Fleck;

namespace popoInputTestFPS.Server;

public class NetworkServer
{
    public event Action<IWebSocketConnection> OnOpen = (s) => { };
    public event Action<IWebSocketConnection> OnClose = (s) => { };
    public event Action<IWebSocketConnection, byte[]> OnBinary = (s, bytes) => { };
    public readonly string url;

    private WebSocketServer webSocketServer;
    private Dictionary<Guid, IWebSocketConnection> connections = new();

    public NetworkServer(string url)
    {
        this.url = url;
        webSocketServer = new(this.url);
    }

    public void Start()
    {
        webSocketServer.Start(socket =>
        {
            socket.OnOpen += () =>
            {
                Console.WriteLine($"OnOpen: {socket.ConnectionInfo.Id}");
                connections[socket.ConnectionInfo.Id] = socket;

                OnOpen.Invoke(socket);
            };
            socket.OnClose += () =>
            {
                Console.WriteLine($"OnClose: {socket.ConnectionInfo.Id}");
                connections.Remove(socket.ConnectionInfo.Id);

                OnClose.Invoke(socket);
            };
            socket.OnBinary += bytes =>
            {
                Console.WriteLine($"OnBinary: {socket.ConnectionInfo.Id}");
                connections[socket.ConnectionInfo.Id] = socket;

                OnBinary.Invoke(socket, bytes);
            };
        });
    }

    public void BroadCast(byte[] bytes)
    {
        foreach (var socket in connections.Values)
            socket.Send(bytes);
    }
}