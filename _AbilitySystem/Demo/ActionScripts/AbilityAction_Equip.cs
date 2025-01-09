using UnityEngine;
public class AbilityAction_Equip : AbilityAction
{
    ActiveAbility _ability;
    [SerializeField] private string _equipSocketName;
    [SerializeField] private float _lerpSpeed = 3f;
    [SerializeField] private EEquipHand _hand;
    
    private Equippable _equippable;

    private bool _startMove;
    public override AbilityAction Clone()
    {
        AbilityAction_Equip clone = AbilityActionPool<AbilityAction_Equip>.Shared.Get();
        clone._equipSocketName = _equipSocketName;
        clone._lerpSpeed = _lerpSpeed;
        clone._hand = _hand;
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
        Transform weaponTransform = Owner.GetData<DS_EquipmentUser>().ItemToEquip.transform;
        _equippable = weaponTransform.GetComponent<Equippable>();
        Transform socket = Owner.GetSocket(_equipSocketName);
        Quaternion firstRot = weaponTransform.rotation;
        weaponTransform.SetParent(socket);
        
        _hasTick = true;
    }
    


    public override void OnTick(Actor owner)
    {
        base.OnTick(owner);
        Transform socket = Owner.GetSocket(_equipSocketName);
        if (socket != null)
        {
            _equippable.transform.position = Vector3.Lerp(_equippable.transform.position, socket.position, _lerpSpeed * Time.deltaTime);
            _equippable.transform.rotation = Quaternion.Lerp(_equippable.transform.rotation, socket.rotation, _lerpSpeed * Time.deltaTime);
        }
    }
    

    public override void OnExit()
    {
        base.OnExit();
        _equippable.transform.localPosition =  Vector3.zero;
        _equippable.transform.rotation = Quaternion.Euler(Vector3.zero);
        Owner.GetData<DS_EquipmentUser>().EquipWorldInstance(Owner.GetData<DS_EquipmentUser>().ItemToEquip, _equipSocketName);
        Owner.GetData<DS_EquipmentUser>().ItemToEquip = null;
    }
}