using Fleck;
using GameServer = JuhaKurisu.PopoTools.InputSystem.Server.Server;

namespace popoInputTestFPS.Server;

public class Program
{
    public static void Main(string[] args) => new Program();

    private WebSocketServer webSocketServer = new WebSocketServer("ws://0.0.0.0:3000");

    private Dictionary<Guid, IWebSocketConnection> connections = new();

    private Program()
    {
        webSocketServer.Start(socket =>
        {
            socket.OnOpen += () =>
            {
                Console.WriteLine($"OnOpen: {socket.ConnectionInfo.Id}");
                connections[socket.ConnectionInfo.Id] = socket;
            };
            socket.OnClose += () =>
            {
                Console.WriteLine($"OnClose: {socket.ConnectionInfo.Id}");
                connections.Remove(socket.ConnectionInfo.Id);
            };
            socket.OnBinary += bytes =>
            {
                Console.WriteLine($"OnBinary: {socket.ConnectionInfo.Id}");
                connections[socket.ConnectionInfo.Id] = socket;
            };
        });

        while (true) ;
    }

    private void BroadCast(byte[] bytes)
    {
        foreach (var socket in connections.Values)
            socket.Send(bytes);
    }
}
