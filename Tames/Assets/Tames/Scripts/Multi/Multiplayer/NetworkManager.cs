using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }

    public static string commandIP;
    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
    }

    private void FixedUpdate()
    {
        Client.Tick();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect()
    {
        if (commandIP == null)
        {
            if (ip.Length > 4)
                Client.Connect($"{ip}:{port}");
            else 
                CoreTame.multiPlayer = false;
        }
        else
            Client.Connect($"{commandIP}:{port}");
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.reliable, Player.Name);
//        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }
    private void DidConnect(object sender, EventArgs e)
    {
        SendName();
        Debug.Log("NM: connected");
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        Debug.Log("NM: failed");
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
    //    Player.PlayerLeft(e.Id);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        Debug.Log("NM: disconnected");

    }
}
