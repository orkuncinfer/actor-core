using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using StatSystem;
using UnityEngine;

public class Network_InventorySync : NetworkBehaviour
{
    [SerializeField] private InventoryDefinition _inventoryToSync;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _inventoryToSync.onInventoryChanged += OnInventoryChanged;
    }

    public override void OnDespawnServer(NetworkConnection connection)
    {
        base.OnDespawnServer(connection);
        _inventoryToSync.onInventoryChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged()
    {
        if(OnStartServerCalled == false)return;
        ItemData[] itemDatas = new ItemData[_inventoryToSync.InventoryData.InventorySlots.Count];
        for (int i = 0; i < _inventoryToSync.InventoryData.InventorySlots.Count; i++)
        {
            if (_inventoryToSync.InventoryData.InventorySlots[i].ItemData == null)
            {
                itemDatas[i] = null;
            }
            else
            {
                itemDatas[i] = _inventoryToSync.InventoryData.InventorySlots[i].ItemData;
            }
        }
        ClientSync(base.Owner,itemDatas);
    }

    [TargetRpc]
    void ClientSync(NetworkConnection connection,ItemData[] itemDatas)
    {
        Debug.Log("Client Sync 1");
        if(!IsOwner || IsServer) return;
        Debug.Log("Client Sync 2");
        for (int i = 0; i < itemDatas.Length; i++)
        {
            _inventoryToSync.InventoryData.InventorySlots[i].ItemData = itemDatas[i];
        }
        _inventoryToSync.TriggerChanged();
    }
}