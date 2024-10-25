using DG.Tweening;
using UnityEngine;

public class AbilityAction_Unequip : AbilityAction
{
    ActiveAbility _ability;
    [SerializeField] private bool _attachToSocket;
    [SerializeField] private string _socketName;
    [SerializeField] private float _lerpSpeed = 3f;
    
    public override AbilityAction Clone()
    {
        AbilityAction_Unequip clone = AbilityActionPool<AbilityAction_Unequip>.Shared.Get();
        
        clone._attachToSocket = _attachToSocket;
        clone._socketName = _socketName;
        clone._lerpSpeed = _lerpSpeed;
        return clone;
    }

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        Debug.Log("Unequip onstart");
        if (Owner.GetEquippedInstance().TryGetComponent(out Equipable equipable))
        {
            equipable.OnUnequip(Owner);
            Transform socket = Owner.GetSocket(_socketName);
            if (socket != null)
            {
                equipable.transform.DORotate(socket.transform.eulerAngles, .5f);
                equipable.transform.DOMove(socket.position, .5f).OnComplete(() =>
                {
                    Owner.GetData<DS_EquipmentUser>().UnequipCurrent(false);
                    if (_attachToSocket)
                    {
                        equipable.transform.SetParent(socket);
                    }
                });
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("Unequip onexit");
        /*if (Owner.GetEquippedInstance().TryGetComponent(out Equipable equipable))
        {
            Owner.GetData<DS_EquipmentUser>().UnequipCurrent();
        }*/
    }
}