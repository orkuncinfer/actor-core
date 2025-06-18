using System;
using UnityEngine;

public class EquippableParentOnEquipped : MonoBehaviour
{
    [SerializeField] private Equippable _equippable;

    [SerializeField] private Transform _transform;

    private void Start()
    {
        if (_equippable.IsEquipped && _equippable.Owner != null)
        {
            OnEquipped(_equippable.Owner);
        }
        
        _equippable.onEquipped += OnEquipped;
        _equippable.onUnequipped += OnUnequipped;
    }

    private void OnDestroy()
    {
        _equippable.onEquipped -= OnEquipped;
        _equippable.onUnequipped -= OnUnequipped;
    }

    private void OnUnequipped(ActorBase obj)
    {
        _transform.SetParent(_equippable.transform);
    }

    private void OnEquipped(ActorBase obj)
    {
        _transform.SetParent(obj.transform);
        _transform.localPosition = Vector3.zero;
        _transform.localEulerAngles = Vector3.zero;
    }
}
