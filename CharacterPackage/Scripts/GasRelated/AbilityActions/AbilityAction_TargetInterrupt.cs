using UnityEngine;

public class AbilityAction_TargetInterrupt : AbilityAction
{
    [SerializeField] private GameplayTagContainer _targetInterruptTags;
    [SerializeField] private bool _interruptIfTargetChange;
    
    private AbilityController _abilityController;
    public override AbilityAction Clone()
    {
        base.Clone();
        AbilityAction_TargetInterrupt clone = AbilityActionPool<AbilityAction_TargetInterrupt>.Shared.Get();
        clone.EventName = EventName;
        clone.AnimWindow = AnimWindow;
        clone._interruptIfTargetChange  = _interruptIfTargetChange;
        clone._targetInterruptTags  = _targetInterruptTags;
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();
        _abilityController = Owner.GetService<Service_GAS>().AbilityController;
        
        if (_abilityController.Target == null)
        {
            RequestEndAbility();
            return;
        }

        if (ActiveAbility.StartTarget == null)
        {
            RequestEndAbility();
            return;
        }

        if (ActiveAbility.StartTarget.GameplayTags.HasAny(_targetInterruptTags))
        {
            RequestEndAbility();
            return;
        }
        
        ActiveAbility.StartTarget.GameplayTags.OnTagChanged += OnTargetTagChanged;
        _abilityController.OnTargetChanged += OnTargetChanged;
    }

    private void OnTargetTagChanged()
    {
        if (ActiveAbility.StartTarget.GameplayTags.HasAny(_targetInterruptTags))
        {
            RequestEndAbility();
            return;
        }
    }

    private void OnTargetChanged(GameObject arg1, GameObject arg2)
    {
        if(!_interruptIfTargetChange)return;
        if (arg2 != ActiveAbility.StartTarget.gameObject)
        {
            RequestEndAbility();
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        ActiveAbility.StartTarget.GameplayTags.OnTagChanged -= OnTargetTagChanged;
        _abilityController.OnTargetChanged -= OnTargetChanged;
        
        AbilityActionPool<AbilityAction_TargetInterrupt>.Shared.Release(this);
    }
}

