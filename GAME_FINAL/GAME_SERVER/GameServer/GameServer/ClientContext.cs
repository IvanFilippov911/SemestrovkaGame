using System.Net.Sockets;

public class ClientContext
{
    public int ClientId { get; set; }
    public TcpClient TcpClient { get; set; }
    public NetworkStream Stream { get; set; }
    public bool IsReady { get; set; } = false;  
    public string Name { get; set; }
    public string Character { get; set; }

    public ClientContext(int clientId, TcpClient tcpClient)
    {
        ClientId = clientId;
        TcpClient = tcpClient;
        Stream = tcpClient.GetStream();
    }
}
