using System;
using FishNet.Object;
using UnityEngine;

public class NetworkInstance : NetworkBehaviour
{
    public static NetworkInstance Instance;

    public event Action onNetworkStarted;
    
    public bool IsNetworkStarted { get; set; }

    public bool IsServer => base.IsServer;

    public bool IsOwner => base.IsOwner;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        onNetworkStarted?.Invoke();
        IsNetworkStarted = true;
    }
}
