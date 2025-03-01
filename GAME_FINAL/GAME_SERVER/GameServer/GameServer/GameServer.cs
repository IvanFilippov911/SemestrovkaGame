using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Numerics;

namespace GameServer;

public class GameServer
{
    private TcpListener server;
    private Thread listenerThread;
    private Dictionary<int, ClientContext> connectedClients = new Dictionary<int, ClientContext>();
    private Dictionary<int, bool> playersReady = new Dictionary<int, bool>();
    private DateTime lastSendTime = DateTime.MinValue;
    private double sendInterval = 200;
    private Queue<string> messageQueue = new Queue<string>();
    private SemaphoreSlim semaphore = new SemaphoreSlim(1);

    public void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        listenerThread = new Thread(ListenForClients);
        listenerThread.Start();
        Console.WriteLine("The server is running...");
    }

    private void ListenForClients()
    {
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Task.Run(() => HandleClient(client));
        }
    }

    private async Task HandleClient(TcpClient tcpClient)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];

        var clientId = tcpClient.GetHashCode();
        var clientContext = new ClientContext(clientId, tcpClient);

        connectedClients.Add(clientId, clientContext);
        playersReady[clientId] = false;
        Console.WriteLine($"Player {clientId} connected");

        while (true)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                await EnqueueMessageAsync(message, clientContext);
            }
            catch
            {
                break;
            }
        }

        connectedClients.Remove(clientId);
        playersReady.Remove(clientId);
        tcpClient.Close();
        Console.WriteLine($"Player {clientId} disconnected");
    }

    private async Task EnqueueMessageAsync(string message, ClientContext clientContext)
    {
        await semaphore.WaitAsync();
        try
        {
            messageQueue.Enqueue(message);
            
        }
        finally
        {
            semaphore.Release();
        }

        await ReceiveMessagesAsync(clientContext);
    }

    private async Task ReceiveMessagesAsync(ClientContext clientContext)
    {
        while (messageQueue.Count > 0)
        {
            await semaphore.WaitAsync();
            try
            {
                if (messageQueue.Count > 0)
                {
                    string message = messageQueue.Dequeue();
                    
                    ProcessMessage(message, clientContext);
                }
            }
            finally
            {
                semaphore.Release();
            }
            await Task.Delay(50);
        }
    }

    private async void ProcessMessage(string message, ClientContext clientContext)
    {
        var clientId = clientContext.ClientId;
        if (message.StartsWith("Position:"))
        {
            if ((DateTime.Now - lastSendTime).TotalMilliseconds >= sendInterval)
            {
                await SendToAllClients($"{message};{clientId}\n", clientId);
                lastSendTime = DateTime.Now;
            }
        }

        else if (message == "Ready")
        {
            playersReady[clientId] = true;
            Console.WriteLine($"Player {clientId} ready!");

            if (playersReady.All(p => p.Value))
            {

                int playerCounter = 0;

                foreach (var id in connectedClients.Keys)
                {
                    Vector3 startPosition = new Vector3(playerCounter * 3, 0, 0);

                    if ((DateTime.Now - lastSendTime).TotalMilliseconds >= sendInterval)
                    {
                        await SendToAllClients($"Position:{startPosition.X};{startPosition.Y};{id}\n", id);
                        lastSendTime = DateTime.Now;
                    }

                    playerCounter++;
                }

                await SendToAllClients("The game begins!");
            }
        }

        else if (message.StartsWith("Attack:"))
        {
            if ((DateTime.Now - lastSendTime).TotalMilliseconds >= sendInterval)
            {
                await SendToAllClients($"{message};{clientId}\n", clientId);

                lastSendTime = DateTime.Now;
            }
        }
        else if (message.StartsWith("PlayerData:"))
        {
            string[] parts = message.Substring(11).Split(';');
            if (parts.Length == 2)
            {
                var name = parts[0].ToString();
                var character = parts[1].ToString();
                
                clientContext.Name = name;
                clientContext.Character = character;
            }
            
            byte[] data = Encoding.ASCII.GetBytes("SuccessPlayerConnect");
            await connectedClients[clientId].Stream.WriteAsync(data, 0, data.Length);
            Console.WriteLine("SuccessPlayerConnect");
        }

    }


    private async Task SendToAllClients(string message, int ignoreId = default)
    {
        foreach (var clientId in connectedClients.Keys)
        {
            if (clientId == ignoreId)
                continue;
            else
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                await connectedClients[clientId].Stream.WriteAsync(data, 0, data.Length);
            }
        }
    }
}
