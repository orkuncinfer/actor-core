using FishNet;
using FishNet.Object;
using UnityEngine;

public class Network_AbilityNetworkLayer : NetworkBehaviour
{
    private ActorBase _actor;
    private Service_GAS _abilitySystem;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _actor = GetComponent<ActorBase>();
        _abilitySystem = _actor.GetService<Service_GAS>();
    }
    
    public void RequestAbility(GameplayTag gameplayTag,int targetNetworkId)
    {
        if(!IsOwner) return;
        if (InstanceFinder.ServerManager.Objects.Spawned.TryGetValue(targetNetworkId, out NetworkObject networkObject))
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.AbilityController.Target = networkObject.gameObject;
                Debug.Log($"Client : Selected target is {networkObject.gameObject.name} for player: {gameObject.name}");
            }
        }
        _abilitySystem.AbilityController.TryActivateAbilityWithGameplayTag(gameplayTag);
        Debug.Log($"Client : started ability for player: {gameObject.name}");
        if (!IsServer)
        {
            RequestAbilityRpc(gameplayTag, targetNetworkId);
        }
    }

    [ServerRpc]
    public void RequestAbilityRpc(GameplayTag gameplayTag,int networkId)
    {
        if(!IsServer) return;
        if (IsServerOnly)
        {
            if (InstanceFinder.ServerManager.Objects.Spawned.TryGetValue(networkId, out NetworkObject networkObject))
            {
                if (_abilitySystem != null)
                {
                    _abilitySystem.AbilityController.Target = networkObject.gameObject;
                    Debug.Log($"Selected target is {networkObject.gameObject.name} for player: {gameObject.name}");
                }
            }
            Debug.Log($"Server : started ability for player: {gameObject.name}");
            _abilitySystem.AbilityController.TryActivateAbilityWithGameplayTag(gameplayTag);
        }
        RequestAbilityObserver(gameplayTag,networkId);
    }
    [ObserversRpc(ExcludeOwner = true)]
    public void RequestAbilityObserver(GameplayTag gameplayTag, int targetNetworkId)
    {
        if(IsServer || IsOwner) return;
        if (InstanceFinder.ServerManager.Objects.Spawned.TryGetValue(targetNetworkId, out NetworkObject networkObject))
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.AbilityController.Target = networkObject.gameObject;
                Debug.Log($"Observer : Selected target is {networkObject.gameObject.name} for player: {gameObject.name}");
            }
        }
        Debug.Log($"Observer : started ability for player: {gameObject.name}");
        _abilitySystem.AbilityController.TryActivateAbilityWithGameplayTag(gameplayTag);
    }
}
