using System.Collections;
using DG.Tweening;
using UnityEngine;

public class AbilityAction_Unequip : AbilityAction
{
    ActiveAbility _ability;
    [SerializeField] private bool _attachToSocket;
    [SerializeField] private string _socketName;
    [SerializeField] private float _lerpSpeed = 3f;
    
    private Equippable _equippable;

    private bool _startMove;
    public override AbilityAction Clone()
    {
        AbilityAction_Unequip clone = AbilityActionPool<AbilityAction_Unequip>.Shared.Get();
        
        clone._attachToSocket = _attachToSocket;
        clone._socketName = _socketName;
        clone._lerpSpeed = _lerpSpeed;
        _hasTick = true;
        return clone;
    }

    public override void Reset()
    {
        base.Reset();
        _equippable = null;
    }

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        
        if (Owner.GetEquippedInstance().TryGetComponent(out Equippable equipable))
        {
            _equippable = equipable;
            Transform socket = Owner.GetSocket(_socketName);
            if (_attachToSocket)
            {
                _equippable.transform.SetParent(socket);
            }
        }
        _equippable.OnUnequip(Owner);
        Owner.GetData<DS_EquipmentUser>().UnequipCurrent(false);
        _hasTick = true;
    }
    


    public override void OnTick(Actor owner)
    {
        base.OnTick(owner);
        Transform socket = Owner.GetSocket(_socketName);
        if (socket != null)
        {
            _equippable.transform.position = Vector3.Lerp(_equippable.transform.position, socket.position, _lerpSpeed * Time.deltaTime);
            _equippable.transform.rotation = Quaternion.Lerp(_equippable.transform.rotation, socket.rotation, _lerpSpeed * Time.deltaTime);
            if (_equippable.transform.position == socket.position)
            {
                if (_attachToSocket)
                {
                    _equippable.transform.SetParent(socket);
                }
            }
        }
    }
    

    public override void OnExit()
    {
        base.OnExit();
        if (_attachToSocket)
        {
            Transform socket = Owner.GetSocket(_socketName);
            _equippable.transform.position = socket.position;
            _equippable.transform.rotation = socket.rotation;
            _equippable.transform.SetParent(socket);
            //Owner.GetData<DS_EquipmentUser>().ItemToEquip = _equippable.gameObject;
        }

        string unequipSlotName = _equippable.ItemDefinition.GetData<Data_Equippable>().UnequipSlotName;
        Owner.SocketRegistry.SlotDictionary[unequipSlotName] = _equippable.transform;
        /*if (Owner.GetEquippedInstance().TryGetComponent(out Equipable equipable))
        {
            Owner.GetData<DS_EquipmentUser>().UnequipCurrent();
        }*/
    }
}