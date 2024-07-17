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
        Debug.Log("collided with " + other.name);
        if (other.transform.TryGetComponent(out Actor actor))
        {
            if(!actor.ContainsTag("Player")) return;
        }
    }

    public void Collect()
    {
        Debug.Log(GeneratedItemResult.ItemId + " collected");
        int added = InventoryDefinition.AddItem(ItemDefinition, DropCount);
        if (added > 0)
        {
            DropCount -= added;
            if (DropCount == 0)
            {
                ItemDropManager.Instance.PickedUp(LabelInstance);
                PoolManager.ReleaseObject(this.gameObject);
            }
        }
    }
}
