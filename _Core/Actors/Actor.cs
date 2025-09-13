using System.Collections.Generic;
using UnityEngine;

public class Actor : ActorBase
{
    [SerializeField] private MonoState _initialState;
    
    [SerializeField] private bool _returnToPoolOnStop = false;
    
    private Dictionary<string,InventoryDefinition> _inventoryDefinitions = new Dictionary<string, InventoryDefinition>();
    protected override void OnActorStart()
    {
        base.OnActorStart();
        if(_initialState)
            _initialState.CheckoutEnter(this);
        
        ActorRegistry.RegisterActor(this);

        foreach (var groupTag in _groupTags)
        {
            ActorRegistry.RegisterActorToGroup(this,groupTag.ID);
        }
    }

    protected override void OnActorStop()
    {
        base.OnActorStop();
        
        foreach (var groupTag in _groupTags)
        {
            ActorRegistry.UnregisterActorFromGroup(this,groupTag.ID);
        }
        
        if (_initialState)
        {
            if (_initialState.IsRunning)
            {
                _initialState.CheckoutExit();
            }
        }
        if (_returnToPoolOnStop)
        {
            PoolManager.ReleaseObject(gameObject);
        }
    }
    
    public void RegisterInventoryDefinition(InventoryDefinition definition)
    {
        _inventoryDefinitions.Add(definition.InventoryId.ID,definition);
        //Debug.Log($"Registered inventory definition {definition.InventoryId.ID} and count is {_inventoryDefinitions.Count}");
    }
    
    public InventoryDefinition GetInventoryDefinition(string inventoryId)
    {
        if (_inventoryDefinitions.ContainsKey(inventoryId))
        {
            return _inventoryDefinitions[inventoryId];
        }
        Debug.LogError($"Couldn't find inventory with id : {inventoryId}");
        return null;
    }
}