using System.Collections;
using ECM2;
using StatSystem;
using UnityEngine;

public class AbilityAction_HitToTarget : AbilityAction
{
    [Tooltip("If true, the target will instantly rotate to face the owner.")]
    public bool SnapTargetRotation;

    public GameplayEffectDefinition EffectToGive;
    public ActiveAbilityDefinition AbilityToGive;
    public ActiveAbilityDefinition TargetDyingAbility;
    public GenericKey DyingAbilityKey;

    private Transform _targetTransform;
    private Attribute _targetHealth;
    private AbilityController _abilityController;
    private Service_GAS _targetGas;
    private GameplayEffectController _effectController;

    private Character _targetCharacter;
    private Character _ownerCharacter;
    private Actor _targetActor;

    private Vector3 _startPosition;
    private float _elapsedTime;

    private AbilityDefinition _beforeDyingAbility;

    public override AbilityAction Clone()
    {
        base.Clone();
        AbilityAction_HitToTarget clone = AbilityActionPool<AbilityAction_HitToTarget>.Shared.Get();
        clone.EventName = EventName;
        clone.AnimWindow = AnimWindow;
        clone._hasTick = true;
        clone.SnapTargetRotation = SnapTargetRotation;
        clone.EffectToGive = EffectToGive;
        clone.AbilityToGive = AbilityToGive;
        clone.TargetDyingAbility = TargetDyingAbility;
        clone.DyingAbilityKey = DyingAbilityKey;    
        
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();

        _abilityController = Owner.GetService<Service_GAS>().AbilityController;
        if (_abilityController.Target == null)
        {
            return;
        }
        _targetTransform = _abilityController.Target.transform;
        _targetActor = _targetTransform.gameObject.GetComponent<Actor>();
        _targetCharacter = _abilityController.Target.GetComponent<Character>();
        _ownerCharacter = Owner.GetComponent<Character>();
        _targetGas = _abilityController.Target.GetComponent<Actor>().GetService<Service_GAS>();
        _targetHealth = _targetGas.StatController.GetAttribute("Health");
        
        if(_targetHealth.CurrentValue <= 0) return;
        
        _startPosition = _ownerCharacter.transform.position;
        Vector3 toTargetXZ = (_targetTransform.position - _startPosition);
        toTargetXZ.y = 0;

        if (toTargetXZ.sqrMagnitude > 0.01f)
        {
            toTargetXZ.Normalize();

            if (SnapTargetRotation && _targetCharacter != null)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-toTargetXZ);
                _targetCharacter.SetRotation(lookRotation);
            }
        }

        if (TargetDyingAbility)
        {
            _beforeDyingAbility = _targetActor.GetData<Data_AbilityDefinition>(DyingAbilityKey.ID).AbilityDefinition;
            _targetActor.GetData<Data_AbilityDefinition>(DyingAbilityKey.ID).AbilityDefinition = TargetDyingAbility;
        }

        if (_targetGas.EffectController.ApplyGameplayEffectDefinition(EffectToGive.ItemID,Owner.gameObject))
        {
            _targetGas.AbilityController.AddAbilityIfNotHave(AbilityToGive);
            _targetGas.AbilityController.TryActiveAbilityWithDefinition(AbilityToGive);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        _targetActor.GetData<Data_AbilityDefinition>(DyingAbilityKey.ID).AbilityDefinition = _beforeDyingAbility;
        AbilityActionPool<AbilityAction_HitToTarget>.Shared.Release(this);
    }
}