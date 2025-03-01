using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class NetworkClient : MonoBehaviour
{
    public UnityAction<string> OnPlayerJoins;

    private TcpClient tcpClient;
    private NetworkStream stream;
    private Vector3 playerPosition;
    [SerializeField]
    private GameObject playerPrefab;
    private Dictionary<int, GameObject> otherPlayers = new Dictionary<int, GameObject>();
    private Vector3 lastPosition;
    public Hero hero;

    private bool isSuccessConnect;
    public bool IsSuccessConnect { get => isSuccessConnect; }

    private float sendInterval = 0.1f;  
    private float lastSendTime = 0f;
    public UnityAction OnReady;

    private bool isStartedGame;
    private bool isThisClientReady;




    public void Initiate(string playerName, string characterChoice)
    {
        DontDestroyOnLoad(gameObject);
        ConnectToServer();

        string message = $"PlayerData:{playerName};{characterChoice}";
        SendMessageToServer(message);

        isThisClientReady = true;
    }

    
    private async void ConnectToServer()
    {
        tcpClient = new TcpClient("127.0.0.1", 5000); 
        stream = tcpClient.GetStream();
        //Debug.Log("The connection is established!");

        
        ReceiveMessagesAsync();
    }

    
    public async void SendMessageToServer(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        await stream.WriteAsync(data, 0, data.Length);  
        await stream.FlushAsync();  
    }



    void Update()
    {
        if (!isThisClientReady)
            return;

        if (Input.GetKeyDown(KeyCode.G)) 
        {
            SendMessageToServer("Ready");
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        if (hero != null)
        {
            playerPosition = hero.transform.position;
        }

        if (playerPosition != lastPosition && Time.time - lastSendTime >= sendInterval)
        {
            //Debug.Log($"Player Position: X = {playerPosition.x}, Y = {playerPosition.y}");
            SendMessageToServer($"Position:{playerPosition.x};{playerPosition.y};{hero.IsRight};{hero.gameObject.name}");
            lastPosition = playerPosition;
            lastSendTime = Time.time;
        }

    }

    
    void OnApplicationQuit()
    {
        stream.Close();
        tcpClient.Close();
    }

    
    private async void ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[1024];

        while (tcpClient.Connected)
        {
            int bytesRead = 0;
            try
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error when receiving data: " + ex.Message);
                break;
            }

            if (bytesRead > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                string[] messages = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var msg in messages)
                {
                    ProcessMessage(msg);
                }
            }
        }
    }

    private void ProcessMessage(string message)
    {
        if (message == "The game begins!")
        {
            OnReady?.Invoke();
            isStartedGame = true;
            Debug.Log("The game begins!");
        }
        if (!isStartedGame) return; 
        if (message.StartsWith("Position:"))
        {
            string[] parts = message.Substring(9).Split(';');
            if (parts.Length == 5)
            {
                float x = float.Parse(parts[0]);
                float y = float.Parse(parts[1]);
                bool isRight = bool.Parse(parts[2]);
                string playerName = parts[3];
                int playerId = int.Parse(parts[4]);
                //Debug.Log($"UPDATE {playerId} at ({x}, {y})");
                
                UpdateOtherPlayerPosition(playerId, new Vector3(x, y, 0), isRight, playerName);
            }
            else
            {
                Debug.LogError("Invalid Position format");
            }
        }
        else if (message.StartsWith("Attack:"))
        {
            string[] parts = message.Substring(7).Split(';');
            if (parts.Length == 6)
            {
                float attackX = float.Parse(parts[0]);
                float attackY = float.Parse(parts[1]);
                float damage = float.Parse(parts[2]);
                bool isRight = bool.Parse(parts[3]);
                float distance = float.Parse(parts[4]);
                int attackerId = int.Parse(parts[5]);

                Vector2 attackPoint = new Vector2(attackX, attackY);

                SimulateOtherPlayerAttack(attackPoint, damage, isRight, distance, attackerId);

                
            }
        }
        else if (message.StartsWith("SuccessPlayerConnect"))
        {
            isSuccessConnect = true;
        }
    }

    private void SimulateOtherPlayerAttack(Vector2 point, float damage, bool isRight, float distance, int attackerId)
    {
        int direction = isRight ? 1 : -1;

        var hit = Physics2D.Raycast(point, Vector2.right * direction, distance);
        if (hit.collider != null)
        {
            hit.collider.GetComponentInParent<Health>()?.TakeDamage(damage);
        }
        otherPlayers[attackerId].GetComponent<Animator>().SetTrigger("IsAttack");
    }

    private void UpdateOtherPlayerPosition(int playerId, Vector3 position, bool isRight, string name)
    {
        if (otherPlayers.ContainsKey(playerId))
        {
            otherPlayers[playerId].transform.position = position;
        }
        else
        {
            GameObject playerObject = Instantiate(playerPrefab);
            playerObject.name = name;
            playerObject.transform.position = position;
           
            playerObject.transform.localScale = Vector3.one;

            otherPlayers[playerId] = playerObject;

            OnPlayerJoins?.Invoke(name);
        }

        otherPlayers[playerId].GetComponentInChildren<SpriteRenderer>().flipX = !isRight;
        //otherPlayers[playerId].GetComponent<Animator>().SetBool("IsMove", true);
    }




}
