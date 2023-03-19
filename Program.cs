using Fleck;
using InputClient = JuhaKurisu.PopoTools.InputSystem.Server.Client;
using InputServer = JuhaKurisu.PopoTools.InputSystem.Server.Server;

namespace popoInputTestFPS.Server;

public class Program
{
    public static void Main(string[] args) => new Program();

    private State state = State.Lobby;
    private NetworkServer networkServer;
    private Dictionary<Guid, InputClient> clients = new Dictionary<Guid, InputClient>();
    private HashSet<Guid> readyPlayers = new HashSet<Guid>();
    private InputServer inputServer;

    private Program()
    {
        networkServer = new NetworkServer("ws://0.0.0.0:3000");
        networkServer.OnOpen += socket =>
        {
            switch (state)
            {
                case State.Lobby:
                    // プレイヤーを追加
                    NewPlayer(socket);

                    // 可視化
                    InspectClients();
                    break;
                case State.Game:
                    // 途中参加は禁止
                    socket.Send(new byte[] { (byte)MessageType.Error });
                    socket.Close();
                    break;
            }
        };
        networkServer.OnClose += socket =>
        {
            switch (state)
            {
                case State.Lobby:
                    // プレイヤーを削除
                    clients.Remove(socket.ConnectionInfo.Id);
                    SendClientsData();
                    InspectClients();
                    break;
                case State.Game:
                    // ゲーム終了
                    FinishGame();
                    break;
            }
        };
        networkServer.OnBinary += (socket, bytes) =>
        {
            switch (state)
            {
                case State.Lobby:
                    // 準備が完了したプレイヤーを記録する
                    if (clients.ContainsKey(socket.ConnectionInfo.Id))
                    {
                        readyPlayers.Add(socket.ConnectionInfo.Id);
                    }
                    break;
                case State.Game:
                    break;
            }
        };

        networkServer.Start();
        while (true) ;
    }

    private void StartGame()
    {
        state = State.Game;
        inputServer = new InputServer(clients.Count, clients);
        readyPlayers = new HashSet<Guid>();
    }

    private void FinishGame()
    {
        state = State.Lobby;
    }

    private void NewPlayer(IWebSocketConnection socket)
    {
        // 公開鍵と秘密鍵を共有
        Guid clientID = socket.ConnectionInfo.Id;
        Guid secretID = Guid.NewGuid();
        List<byte> bytes = new List<byte>();

        bytes.Add((byte)MessageType.KeyShare);
        bytes.AddRange(clientID.ToByteArray());
        bytes.AddRange(secretID.ToByteArray());

        socket.Send(bytes.ToArray());

        clients[clientID] = new InputClient(clientID, secretID, new byte[1]);

        //全体に全員の情報を送る
        SendClientsData();
    }

    private void InspectClients()
    {
        foreach (var id in clients.Keys)
        {
            Console.WriteLine($"    {id}");
        }
    }

    private void SendClientsData()
    {
        List<byte> bytes = new List<byte>();

        foreach (var id in clients.Keys)
            bytes.AddRange(id.ToByteArray());

        networkServer.BroadCast(bytes.ToArray());
    }
}
