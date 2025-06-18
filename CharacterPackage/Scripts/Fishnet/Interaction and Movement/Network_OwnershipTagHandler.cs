using System;
using FishNet.Object;
using UnityEngine;

public class Network_OwnershipTagHandler : NetworkBehaviour
{
    private Actor _actor;

    [SerializeField] private GameplayTag _ownerTag;
    [SerializeField] private GameplayTag _serverTag;
    [SerializeField] private GameplayTag _serverOnlyTag;

    private void Awake()
    {
        _actor = GetComponent<Actor>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            _actor.GameplayTags.AddTag(_ownerTag);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (base.IsServer)
        {
            _actor.GameplayTags.AddTag(_serverTag);
        }

        if (base.IsServerOnly)
        {
            _actor.GameplayTags.AddTag(_serverOnlyTag);
        }
    }
}
