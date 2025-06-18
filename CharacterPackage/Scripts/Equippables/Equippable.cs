using System;
using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;


public class Equippable : MonoBehaviour
{
    [SerializeField][ReadOnly]
    private Actor _owner; 
    public Actor Owner 
    {
        get => _owner;
        set => _owner = value;
    }

    public ItemDefinition ItemDefinition;
    public ItemData ItemData; // unique data of equippable
    
    [SerializeField]
    private ScriptableObject _overrideLocomotionAsset;
    
    [SerializeField] private string _equipSocketName;
    [SerializeField] private string _unequipSocketName;
    
    [SerializeField] private AbilityDefinition _equipAbility;
    [SerializeField] private AbilityDefinition _unequipAbility;
    [SerializeField] private AbilityDefinition[] _grantedAbilities;
    public event Action<ActorBase> onEquipped; 
    public event Action<ActorBase> onUnequipped;
    public string EquipSocketName => _equipSocketName;
    
    public GameplayTagContainer EquipTags;
    
    private Object _previousLocomotionAsset;

    public bool IsEquipped => _isEquipped;
    private bool _isEquipped;
    
    public virtual void OnEquip(Actor owner)
    {
        _owner = owner;
        Owner.GetData<DS_EquipmentUser>().SocketName = _equipSocketName;
        if (_overrideLocomotionAsset != null)
        {
            _previousLocomotionAsset = owner.GetData<Data_RefVar>("Locomotion").Value;
            _owner.GetData<Data_RefVar>("Locomotion").Value = _overrideLocomotionAsset;
        }
        
        _isEquipped = true;
        Owner.GameplayTags.AddTags(EquipTags);
        foreach (var abilityDefinition in _grantedAbilities)
        {
            Owner.GetService<Service_GAS>().AbilityController.AddAbilityIfNotHave(abilityDefinition);
        }
        onEquipped?.Invoke(owner);
    }

    public virtual void OnUnequip(ActorBase actor)
    {
        if (_overrideLocomotionAsset != null)
        {
            _owner.GetData<Data_RefVar>("Locomotion").Value = _previousLocomotionAsset;
        }
        _isEquipped = false;
        Owner.GameplayTags.RemoveTags(EquipTags);
        foreach (var abilityDefinition in _grantedAbilities)
        {
            Owner.GetService<Service_GAS>().AbilityController.RemoveAbilityIfHave(abilityDefinition);
        }
        onUnequipped?.Invoke(Owner);
    }

    public virtual void UnequipToSocket()
    {
        Transform socket = Owner.GetSocket(_unequipSocketName);
        transform.position = socket.position;
        transform.rotation = socket.rotation;
        transform.SetParent(socket);
        //Owner.GetData<DS_EquipmentUser>().ItemToEquip = _equippable.gameObject;

        string unequipSlotName = ItemDefinition.GetData<Data_Equippable>().UnequipSlotName;
        Owner.SocketRegistry.SlotDictionary[unequipSlotName] = transform;
    }
    public virtual void EquipThisInstance(Actor actor)
    {
        Owner = actor;
        Owner.GetData<DS_EquipmentUser>().EquipWorldInstance(gameObject,EquipSocketName);
    }
    public ActiveAbility TryUnequipWithAbility()
    {
        return Owner.GetService<Service_GAS>().AbilityController.AddAndTryActivateAbility(_unequipAbility);
    }
    public ActiveAbility TryEquipWithAbility()
    {
        return Owner.GetService<Service_GAS>().AbilityController.AddAndTryActivateAbility(_equipAbility);
    }
}
