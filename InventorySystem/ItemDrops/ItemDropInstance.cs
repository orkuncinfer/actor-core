using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropInstance : MonoBehaviour
{
    public ItemDefinition ItemDefinition;
    public InventoryDefinition InventoryDefinition;
    public int DropCount;
    
    [SerializeField] private GOPoolMember _goPoolMember;

    private void OnEnable()
    {
        if(ItemDefinition.Prefab)
            Instantiate(ItemDefinition.Prefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collided with " + other.name);
        if (other.transform.TryGetComponent(out Actor actor))
        {
            if(!actor.ContainsTag("Player")) return;
            InventoryDefinition.AddItem(ItemDefinition, DropCount);
            _goPoolMember.ReturnToPool();
        }
    }
}
