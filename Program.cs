using InputServer = JuhaKurisu.PopoTools.InputSystem.Server.Server;

namespace popoInputTestFPS.Server;

public class Program
{
    public static void Main(string[] args) => new Program();

    private GameServer gameServer;

    private Program()
    {
        gameServer = new GameServer("ws://0.0.0.0:3000");

        while (true) ;
    }
}
