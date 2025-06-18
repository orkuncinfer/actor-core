using System.Collections.Generic;
using Core;
using FishNet.Connection;
using FishNet.Object;
using StatSystem;
using UnityEngine;

public class Network_HandleDamageNumbers : NetworkBehaviour
{
    [SerializeField] private GameObject _floatingTextPrefab;
    private List<Actor> _registeredList = new List<Actor>();
    
    public void RegisterAsTarget(Actor actor)
    {
        if(!base.IsServer) return;
        if(_registeredList.Contains(actor)) return;
        
        _registeredList.Add(actor);
        actor.GetService<Service_GAS>().StatController.GetAttribute("Health").onAppliedModifier += OnAppliedMod;
    }
    
    public void UnregisterAsTarget(Actor actor)
    {
        if(!base.IsServer) return;
        if(!_registeredList.Contains(actor)) return;
        
        _registeredList.Remove(actor);
        actor.GetService<Service_GAS>().StatController.GetAttribute("Health").onAppliedModifier -= OnAppliedMod;
    }
    
    private void OnAppliedMod(StatModifier obj,StatController controller, int difference)
    {
        if (obj.Source is GameplayEffect effect)
        {
            if(effect.Source != gameObject) return;
            Debug.Log($"Damaged by {effect.Source}");
        }
        
        Show(base.Owner,controller.gameObject,difference);
    }

    [TargetRpc]
    private void Show(NetworkConnection connection,GameObject target,int difference)
    {
        if(!base.IsOwner) return;
        
        GameObject instance = PoolManager.SpawnObject(_floatingTextPrefab,target.transform.position + Vector3.up * 2,Quaternion.identity);
        FloatingText floatingText = instance.GetComponent<FloatingText>();
        
        if (difference > 0)
        {
            floatingText.Set("+" + difference.ToString(),Color.green);
            floatingText.Animate();
        }
        else
        {
            floatingText.Set(difference.ToString(),Color.red);
            floatingText.Animate();
        }
    }
}
