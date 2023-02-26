using Fleck;

namespace popoInputTestFPS.Server;

public class GameServer
{
    public event Action OnOpen = () => { };
    public event Action OnClose = () => { };
    public event Action<byte[]> OnBinary = bytes => { };
    public readonly string url;

    private WebSocketServer webSocketServer;
    private Dictionary<Guid, IWebSocketConnection> connections = new();

    public GameServer(string url)
    {
        this.url = url;
        webSocketServer = new(this.url);

        webSocketServer.Start(socket =>
        {
            socket.OnOpen += () =>
            {
                Console.WriteLine($"OnOpen: {socket.ConnectionInfo.Id}");
                connections[socket.ConnectionInfo.Id] = socket;

                OnOpen.Invoke();
            };
            socket.OnClose += () =>
            {
                Console.WriteLine($"OnClose: {socket.ConnectionInfo.Id}");
                connections.Remove(socket.ConnectionInfo.Id);

                OnClose.Invoke();
            };
            socket.OnBinary += bytes =>
            {
                Console.WriteLine($"OnBinary: {socket.ConnectionInfo.Id}");
                connections[socket.ConnectionInfo.Id] = socket;

                OnBinary.Invoke(bytes);
            };
        });
    }

    public void BroadCast(byte[] bytes)
    {
        foreach (var socket in connections.Values)
            socket.Send(bytes);
    }
}