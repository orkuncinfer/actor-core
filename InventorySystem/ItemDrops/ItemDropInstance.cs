using System;
using System.Collections;
using System.Collections.Generic;
using NetworkShared.Packets.ServerClient;
using Sirenix.OdinInspector;
using UnityEngine;

public class ItemDropInstance : MonoBehaviour
{
    public ItemDefinition ItemDefinition;
    public InventoryDefinition InventoryDefinition;
    public GenericKey InventoryKey;
    public int DropCount;
    public WorldItemLabel LabelInstance;
    
    [ShowInInspector]public FbGeneratedItemResult GeneratedItemResult;

    public void OnMovementEnd()
    {
        LabelInstance.ActivateLabel();
    }

    private void OnEnable()
    {
        if(ItemDefinition.WorldPrefab)
            Instantiate(ItemDefinition.WorldPrefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out Actor actor))
        {
            if(!actor.ContainsTag("Player")) return;
        }
    }
    [Button]
    public void Collect()
    {
        InventoryDefinition collectInventory = null;
        if(ItemDefinition) collectInventory = InventoryDefinition;
        if (InventoryKey) collectInventory = DefaultPlayerInventory.Instance.GetInventoryDefinition(InventoryKey.ID);
      
        int added = collectInventory.AddItem(ItemDefinition, DropCount);
        if (added > 0)
        {
            DropCount -= added;
            if (DropCount == 0)
            {
                if(LabelInstance)ItemDropManager.Instance.PickedUp(LabelInstance);
                PoolManager.ReleaseObject(this.gameObject);
            }
        }
    }
}
