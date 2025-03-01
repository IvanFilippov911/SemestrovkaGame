namespace GameServer;

internal class Program
{
    static void Main(string[] args)
    {
        GameServer server = new GameServer();
        server.StartServer();
    }
}
