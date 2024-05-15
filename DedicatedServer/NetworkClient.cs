using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkShared;
using NetworkShared.Packets.ClientServer;
using NetworkShared.Packets.ServerClient;
using NetworkShared.Registries;
using Sirenix.OdinInspector;
using UnityEngine;

public class NetworkClient : MonoBehaviour, INetEventListener
{
    private static NetworkClient _instance;
    public static NetworkClient Instance => _instance;
    [SerializeField] private bool _connectLocal;
    public bool IsConnected;
    
    private NetManager _netManager;
    private NetPeer _server;
    private NetDataWriter _writer;
    private PacketRegistry _packetRegistry;
    private HandlerRegistry _handlerRegistry;

    public Net_GeneratedItemResult ItemResult;

    [ShowInInspector]private Dictionary<string, INetPacket> PendingRequests = new Dictionary<string, INetPacket>();
    public event Action onServerConnected;
    public event Action onServerDisconnected;
    public event Action<INetPacket> onPacketReceived;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        Init();
    }

    private void Start()
    {
        //Init();
    }

    private void OnDestroy()
    {
        if (_server != null)
        {
            _netManager.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void Update()
    {
        _netManager.PollEvents();
    }

    public void Init()
    {
        _handlerRegistry = new HandlerRegistry();
        _packetRegistry = new PacketRegistry();
        _writer = new NetDataWriter();
        _netManager = new NetManager(this)
        {
            DisconnectTimeout = 100000
        };
        _netManager.Start();
    }

    [Button("Test Connect")] 
    public void Connect()
    {
        if (_connectLocal) 
            _netManager.Connect("localhost", 9050, "");
        else  
            _netManager.Connect("209.38.146.136", 9050, "");
    }
    
    [Button("LocalConnectWithTestId")]
    public void TestConnectWithTestId()
    {
        _netManager.Connect("localhost", 9050, "test");
        
        var authRequest = new Net_AuthRequest
        {
            Username = "JUL1vonO6EWSM2uDdl63FJuUZwP2",
            Password = "test"
        };
        
        SendServer(authRequest);
    }

    [Button]
    public void TestAuth()
    {
        var authRequest = new Net_AuthRequest
        {
            Username = "JUL1vonO6EWSM2uDdl63FJuUZwP2",
            Password = ""
        };
        NetworkClient.Instance.SendServer(authRequest);
    }
    public async Task DropItemAsync()
    {
        var generatedItem = new Net_GenerateItemRequest()
        {
            MonsterId = "mns_golem",
            RequestId = GenerateUniqueRequestId()
        };

        var result = await SendServerAsync<Net_GenerateItemRequest, Net_GeneratedItemResult>(generatedItem);
        
    }
    [Button]
    public async void DropItem()
    {
        var generatedItem = new Net_GenerateItemRequest()
        {
            MonsterId = "mns_golem",
            RequestId = GenerateUniqueRequestId()
        };
        
        try
        {
            var result = await SendServerAsync<Net_GenerateItemRequest, Net_GeneratedItemResult>(generatedItem);
            if (result is Net_GeneratedItemResult itemResult)
            {
                Debug.Log("Received result for RequestId: " + itemResult.RequestId +"-"+ itemResult.ItemId);
            }
            
            // Handle the result here
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to receive response: " + e.Message);
        }
        Debug.Log("continue debugging");
    }
    
    public void Disconnect()
    {
        _netManager.DisconnectAll();
    }

    public void SendServer<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : INetSerializable
    {
        if (_server == null) return;
        _writer.Reset();
        packet.Serialize(_writer);
        _server.Send(_writer, deliveryMethod);
    }

    

    public string GenerateUniqueRequestId()
    {
        string requestId = Guid.NewGuid().GetHashCode().ToString();
        Debug.Log("Generating unique request ID : " + requestId);
        return requestId;
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("Connected to server as " + peer.Id);
        _server = peer;
        onServerConnected?.Invoke();
        IsConnected = true;
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        onServerDisconnected?.Invoke();
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var packetType = (PacketType)reader.GetByte();
        var packet = ResolvePacket(packetType, reader);
        var handler = ResolveHandler(packetType);
        handler.Handle(packet, peer.Id);
        onPacketReceived?.Invoke(packet);
        reader.Recycle();
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
    }

    private IPacketHandler ResolveHandler(PacketType packetType)
    {
        var handlerType = _handlerRegistry.Handlers[packetType];
        return (IPacketHandler)Activator.CreateInstance(handlerType);
    }

    private INetPacket ResolvePacket(PacketType packetType, NetPacketReader reader)
    {
        var type = _packetRegistry.PacketTypes[packetType];
        var packet = (INetPacket)Activator.CreateInstance(type);
        packet.Deserialize(reader);
        return packet;
    }

    public async Task<INetPacket> SendServerAsync<TRequest, TResponse>(TRequest packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        where TRequest : INetSerializable
        where TResponse : INetSerializable
    {
        string requestId = "";

        // Add the request ID to the packet if necessary
        if (packet is Net_GenerateItemRequest requestPacket)
        {
            requestId = requestPacket.RequestId;
            Debug.Log($"Added RequestId: {requestId} to packet");
        }

        // Store the TaskCompletionSource in a dictionary to retrieve it later
        PendingRequests.Add(requestId, null);
        Debug.Log($"Added request with RequestId: {requestId}");

        // Send the packet to the server
        SendServer(packet, deliveryMethod);


        while (PendingRequests[requestId] == null)
        {
            await Task.Yield();
        }
        var result = PendingRequests[requestId];
        PendingRequests.Remove(requestId);
        return result;
    }
    public void CompleteRequest<TResponse>(string requestId, TResponse response) where TResponse : class, INetPacket
    {
        //StartCoroutine(DelayedComplete(requestId, response));
       // return;
        if (PendingRequests.TryGetValue(requestId, out var tcs))
        {
            PendingRequests[requestId] = response;
            Debug.Log($"Completed request with RequestId: {requestId}");
        }
        else
        {
            Debug.LogWarning($"No pending request found for RequestId: {requestId}");
        }
    }

    IEnumerator DelayedComplete(string requestId, INetPacket response)
    {
        yield return new WaitForSeconds(5);
        if (PendingRequests.TryGetValue(requestId, out var tcs))
        {
            PendingRequests[requestId] = response;
            Debug.Log($"Completed request with RequestId: {requestId}");
        }
        else
        {
            Debug.LogWarning($"No pending request found for RequestId: {requestId}");
        }
    }
}
