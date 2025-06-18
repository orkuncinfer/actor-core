using FishNet.Object;
using StatSystem;
using UnityEngine;
using FishNet.Object;
using StatSystem;
using UnityEngine;

public class Network_StatSync : NetworkBehaviour
{
    private StatController _statController;

    public string[] SyncedAttributes;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _statController = GetComponentInChildren<StatController>();
        _statController.onStatIsModified += OnStatModified;

        /*foreach (var attr in SyncedAttributes)
        {
            _statController.GetAttribute(attr).onAttributeChanged += OnAttributeChanged;
        }*/
    }

    private void OnStatModified(Stat obj)
    {
        if (base.IsServerOnly)
        {
            if (obj is StatSystem.Attribute attr)
            {
                ObserverStatModified(obj.Definition.name,attr.Value,attr.CurrentValue);
            }
            else
            {
                ObserverStatModified(obj.Definition.name,obj.Value);
            }
        }
    }
    
    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    private void ObserverStatModified(string statName, int value, int currentValue = -1)
    {
        if(base.IsServer) return;
        
        Stat theStat = _statController.GetStat(statName);
        if (theStat is StatSystem.Attribute attribute)
        {
            Debug.Log($"Attribute {statName} is updated from {attribute.CurrentValue } to : {value}");
            attribute.CurrentValue = currentValue;
            attribute.SetValue(value);
        }
        else
        {
            theStat.SetValue(value);
        }
    }
}
